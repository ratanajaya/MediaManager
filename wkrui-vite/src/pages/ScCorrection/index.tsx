import { Alert, Button, Checkbox, Select, Space, Tag, Typography } from 'antd';
import { useEffect, useMemo, useState } from 'react'
import { CorrectPageParam, FileCorrectionModel, FileCorrectionReportModel, KeyValuePair, PathCorrectionModel, QueryPart, SignalrMessage, SignalrMessageType } from '_utils/Types';
import _uri from '_utils/_uri';
import _helper from "_utils/_helper";
import { LoadingOutlined } from '@ant-design/icons';
import _constant from '_utils/_constant';
import { useAuth } from '_shared/Contexts/useAuth';
import useNotification from '_shared/Contexts/NotifProvider';
import * as signalR from '@microsoft/signalr';

const { Text } = Typography;

interface IScCorrectionProps{
  queryParts: QueryPart,
}

const contentPad = {
  paddingLeft:'12px', paddingRight:'12px'
}

export default function ScCorrection(props: IScCorrectionProps) {
  const { axiosA } = useAuth();
  const { notif } = useNotification();
  const [paths, setPaths] = useState<PathCorrectionModel[]>([]);
  const [selectedPath, setSelectedPath] = useState<string | null>(null);
  const [selectedRes, setSelectedRes] = useState<number>(1280);
  const [selectedThread, setSelectedThread] = useState<number>(2);
  const [upscalerOptions, setUpscalerOptions] = useState<any[]>([]);
  const [selectedUpscaler, setSelectedUpscaler] = useState<number | null>(301);
  const [limitToCorrectablePath, setLimitToCorrectablePath] = useState<boolean>(false);
  const [toWebp, setToWebp] = useState<boolean>(false);
  const [clampToTarget, setClampToTarget] = useState<boolean>(false);

  const [fileToCorrect, setFileToCorrect] = useState<FileCorrectionModel[]>([]);
  const [reports, setReports] = useState<FileCorrectionReportModel[]>([]);
  const [opStatus, setOpStatus] = useState<{apiLoading: boolean, ongoingOperation: boolean, message: string}>({ 
    apiLoading: false,
    ongoingOperation: false,
    message: '' 
  });

  const hPath = props.queryParts.path;
  const isSingleAlbumMode = !_helper.isNullOrEmpty(hPath);

  useEffect(() => {
    setOpStatus(prev => ({
      ...prev,
      apiLoading: true,
      message: 'Loading Albums...'
    }));

    axiosA.get<KeyValuePair<number,string>[]>(_uri.GetUpscalers())
      .then((response) => {
        setUpscalerOptions(response.data.map(a => ({
          value: a.key,
          label: a.value
        })));
      })
      .catch((error) => {
        notif.apiError(error);
      });

    if(isSingleAlbumMode){
      //reloadFileToCorrect(hPath, selectedThread, selectedRes);
    }
    else{
      axiosA.get<PathCorrectionModel[]>(_uri.ScGetCorrectablePaths())
        .then((response) => {
          setPaths(response.data);
        })
        .catch((error) => {
          notif.apiError(error);
        })
        .finally(() => {
          setOpStatus(prev => ({
            ...prev,
            apiLoading: false,
            message: 'Albums loaded'
          }));
        });
    }
  },[]);

  function fullScan(){
    setOpStatus(prev => ({
      ...prev,
      apiLoading: true,
      message: 'Scanning the library for correctable pages...'
    }));

    axiosA.post<PathCorrectionModel[]>(_uri.ScFullScanCorrectiblePaths(), { }, { params:{thread:selectedThread, upscaleTarget:selectedRes } })
      .then((response) => {
        setPaths(response.data);
      })
      .catch((error) => {
        notif.apiError(error);
      })
      .finally(() => {
        setOpStatus(prev => ({
          ...prev,
          apiLoading: false,
          message: 'Library scan finished'
        }));
      });
  }

  const pathToDisplays = paths.filter(a => !limitToCorrectablePath || a.correctablePageCount > 0);

  const upscaleOptionDisplay = pathToDisplays.map(a => ({
    value: a.libRelPath,
    label: (
      <div style={{display:'flex'}}>
        <div style={{flex:'1'}}>{a.libRelPath}</div>
        <div style={{width:'50px', textAlign:'right'}}>{`[${a.correctablePageCount}]`}</div>
        <div style={{width:'160px', paddingRight:'4px', textAlign:'right'}}>{_helper.formatNullableDatetime(a.lastCorrectionDate)}</div>
      </div>)
  }));

  function reloadFileToCorrect(path: string | null, thread: number, res: number, clampToTarget: boolean) {
    if(!path) return;

    setOpStatus(prev => ({
      ...prev,
      apiLoading: true,
      message: 'Loading files...'
    }));

    const type = isSingleAlbumMode ? 0 : 1;

    axiosA.get<FileCorrectionModel[]>(_uri.GetCorrectablePages(type, path, thread, res, clampToTarget))
      .then((response) => {
        setFileToCorrect(response.data);

        if(!isSingleAlbumMode){
          setPaths(prev => {
            const foundIdx = prev.findIndex(a => a.libRelPath === path);
            prev[foundIdx].correctablePageCount = response.data.length;

            return [...prev];
          });
        }

        setReports([]);
      })
      .catch((error) => {
        notif.apiError(error);
      })
      .finally(() => {
        setOpStatus(prev => ({
          ...prev,
          apiLoading: false,
          message: 'Files loaded'
        }));
      });
  }

  useEffect(() => {
    if(isSingleAlbumMode){
      reloadFileToCorrect(hPath, selectedThread, selectedRes, clampToTarget);
    }
    else{
      reloadFileToCorrect(selectedPath, selectedThread, selectedRes, clampToTarget);
    }
  }, [selectedPath, selectedRes, clampToTarget]);

  function handleSubmit(){
    const {path, type} = isSingleAlbumMode ? { path: hPath, type: 0 } : { path: selectedPath!, type: 1 };

    if(path == null) 
      return;

    if(selectedUpscaler == null) {
      notif.error('Please select an upscaler', 'Upscaler not selected');
      return;
    }

    setOpStatus(prev => ({
      ...prev,
      apiLoading: true,
      message: 'Fetching operation id for queued files...'
    }));

    const param: CorrectPageParam = {
      type: type,
      libRelPath: path,
      thread: selectedThread,
      upscalerType: selectedUpscaler,
      fileToCorrectList: fileToCorrect,
      toWebp: toWebp,
    };

    axiosA.post<string>(_uri.CorrectPagesSignalr(), param)
      .then(async (response) => {
        setOpStatus({
          apiLoading: false,
          ongoingOperation: true,
          message: `Correction ongoing with Id: ${response.data}`
        });

        const connection = new signalR.HubConnectionBuilder()
          .withUrl(_uri.ProgressHub(), {
            withCredentials: false
          })
          .configureLogging(signalR.LogLevel.Information)
          .build();

        connection.on("ReceiveProgress", (message: SignalrMessage<FileCorrectionReportModel>) => {
          //console.log('ReceiveProgress', message);
          if(message.messageType === SignalrMessageType.Progress){
            setReports(prev => {
              return [...prev, message.data!];
            });
          }
          else if(message.messageType === SignalrMessageType.Complete || message.messageType === SignalrMessageType.Error){
            setOpStatus(prev => ({ 
              ...prev, 
              ongoingOperation: false,
              message: message.message ?? ''
            }));

            connection.stop();
          }
        });

        try{
          await connection.start();

          await connection.invoke("JoinOperationGroup", response.data);
        }
        catch (err) {
          setOpStatus(prev => ({ 
            ...prev,
            ongoingOperation: false,
            message: `Error occured ${JSON.stringify(err)}` 
          }));
        }
      }).catch((error) => {
        notif.apiError(error);

        setOpStatus(prev => ({ 
          ...prev, 
          apiLoading: false,
          message: `Error occured ${JSON.stringify(error)}`
        }));
      });
  }

  const fileToCorrectSorted = fileToCorrect.sort((a, b) => (a.correctionType ?? 0) - (b.correctionType ?? 0) || a.alRelPath.localeCompare(b.alRelPath));
  const uLength = fileToCorrectSorted.filter(a => a.correctionType === 1).length;
  const cLength = fileToCorrectSorted.length - uLength;

  const ftcByteSum = fileToCorrect.map(a => a.byte).reduce((prev, next) => prev + next, 0);
  const ftcByteAvg = fileToCorrect.length > 0 ? ftcByteSum / fileToCorrect.length : 0;

  const rptByteSum = reports.map(a => a.byte).reduce((prev, next) => prev + next, 0);
  const rptByteAvg = reports.length > 0 ? rptByteSum / reports.length : 0;

  const memoizedFileList = useMemo(() => { 
    return (
      <div>
        {fileToCorrectSorted.map(a =>{ 
          const report = reports.find(c => c.alRelPath === a.alRelPath);

          return(
            <FileDisplay
              key={a.alRelPath}
              file={a}
              ongoingOperation={opStatus.ongoingOperation}
              report={report}
            />
          );
        })}
      </div>
    );
  }, [fileToCorrectSorted, reports]);

  const somethingOngoing = opStatus.apiLoading || opStatus.ongoingOperation;

  return (
    <Space direction='vertical' style={{width:'100%'}}>
      {!isSingleAlbumMode && 
        <div style={{display:'flex'}}>
          <div style={{flex:'1px'}}>
            <Select 
              style={{width:'100%'}}
              options={upscaleOptionDisplay}
              value={selectedPath}
              onSelect={value => { setSelectedPath(value); }}
              disabled={somethingOngoing || isSingleAlbumMode}
            />
          </div>
          <div style={{width:'8px'}}></div>
          <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
            <Checkbox 
              checked={limitToCorrectablePath} 
              onChange={(e) => setLimitToCorrectablePath(e.target.checked)}
              disabled={somethingOngoing || isSingleAlbumMode}
            >
              Limit
            </Checkbox>
          </div>
          <div style={{width:'8px'}}></div>
          <div style={{flex:'1', maxWidth:'120px'}}>
            <Button 
              style={{width:'100%'}} onClick={fullScan}
              disabled={somethingOngoing || isSingleAlbumMode}
            >
              Full Scan
            </Button>
          </div>
        </div>
      }
      <div style={{display:'flex'}}>
        <div style={{flex:'1px'}}>
          <Select 
            style={{width:'100%'}}
            options={upscalerOptions}
            value={selectedUpscaler} onChange={val => setSelectedUpscaler(val)}
            disabled={somethingOngoing}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Select 
            style={{width:'100%'}}
            options={_constant.threadOptions}
            value={selectedThread} onChange={val => setSelectedThread(val)}
            disabled={somethingOngoing}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Select 
            style={{width:'100%'}}
            options={_constant.resOptions}
            value={selectedRes} onChange={val => setSelectedRes(val)}
            disabled={somethingOngoing}
          />
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
          <Checkbox 
            checked={clampToTarget} 
            onChange={(e) => setClampToTarget(e.target.checked)}
            disabled={somethingOngoing}
          >
            Clamp
          </Checkbox>
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px', display: 'flex', alignItems: 'center', paddingLeft:'12px'}}>
          <Checkbox 
            checked={toWebp} 
            onChange={(e) => setToWebp(e.target.checked)}
            disabled={somethingOngoing}
          >
            To Webp
          </Checkbox>
        </div>
        <div style={{width:'8px'}}></div>
        <div style={{flex:'1', maxWidth:'120px'}}>
          <Button 
            style={{width:'100%'}} onClick={handleSubmit}
            disabled={somethingOngoing}>
            Submit
          </Button>
        </div>
      </div>
      {!_helper.isNullOrEmpty(opStatus.message) &&
        <Alert 
          message={
            <div>
              {somethingOngoing && <LoadingOutlined style={{marginRight:'8px'}} /> }
              {opStatus.message}
            </div>
          }
          type={somethingOngoing ? 'warning' : 'info'} 
        />
      }
      <div style={{width:'100%', display:'flex', ...contentPad}}>
        <div style={{flex:'2'}}><Text>U: {uLength}    C: {cLength}</Text></div>
        <div style={{width:'220px', display:'flex'}}>
          <div style={{width:'30px'}}></div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>Sum: {_helper.formatBytes2(ftcByteSum)}</Text></div>
            <div><Text>Avg: {_helper.formatBytes2(ftcByteAvg)}</Text></div>
          </div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>Sum: {_helper.formatBytes2(rptByteSum)}</Text></div>
            <div><Text>Avg: {_helper.formatBytes2(rptByteAvg)}</Text></div>
          </div>
        </div>
      </div>
      {memoizedFileList}
      <div className='divider-8'></div>
    </Space>
  );
}

function FileDisplay(props:{
  file: FileCorrectionModel,
  ongoingOperation: boolean,
  report?: FileCorrectionReportModel
}){

  const { file, report } = props;
  
  return (
    <div style={{width:'100%'}}>
      <div className='divider-4'></div>
      <div style={{width:'100%', display: 'flex', ...contentPad}}>
        <div style={{flex:'2'}}><Text>{file.alRelPath}</Text></div>
        <div style={{width:'220px', display:'flex'}}>
          <div style={{width: '30px'}}>
            {props.ongoingOperation && report == null 
              ? <LoadingOutlined />
              : report == null
              ? <></>
              : <Tag color={report?.success ? 'blue' : 'red'}>{report?.success ? 'S' : 'F'}</Tag>
            }
          </div>
          <div style={{width:'30px'}}>
            {file.correctionType === 1 
              ? <Tag color="green">U</Tag> 
              : <Tag color="orange">C</Tag>
            }
          </div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>{_helper.formatBytes2(file.byte)}</Text>  <Text strong>{file.bytesPer100Pixel}</Text></div>
            <div><Text>{file.width} x {file.height}</Text></div>
          </div>
          <div style={{flex:'1', textAlign:'end'}}>
            <div><Text>{_helper.formatBytes2(report?.byte)}</Text>  <Text strong>{report?.bytesPer100Pixel}</Text></div>
            <div><Text>{file.compression.width} x {file.compression.height}</Text></div>
          </div>
        </div>
      </div>
      {(report != null && !report.success) && 
        <Alert message={report.message} type="error" />
      }
    </div>
  );
}