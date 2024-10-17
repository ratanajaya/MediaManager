import { Tooltip, Typography } from 'antd';
import { useEffect, useState } from 'react';
import cssVariables from '_assets/styles/cssVariables';
import _db from '_utils/_db';
import { AlbumFsNodeInfo, AlbumCardModel, FsNode } from '_utils/Types';
import { useAuth } from '_shared/Contexts/useAuth';
import _helper from '_utils/_helper';
import { CheckOutlined, CloseOutlined, HourglassOutlined, SyncOutlined } from '@ant-design/icons';
import _uri from '_utils/_uri';
import useSWR from 'swr';

interface FileDownload {
  id: string,
  albumPath: string,
  alRelPath: string,
  downloadState: 'pending' | 'downloading' | 'finished' | 'error',
  errorMessage?: string
}

function MapFsNodeToFileDownload(src: FsNode, albumPath: string){
  return {
    id: `${albumPath}/${src.alRelPath}`,
    albumPath: albumPath,
    alRelPath: src.alRelPath,
    downloadState: 'pending'
  } as FileDownload;
}

export function AlbumDownloadModal(props: {
  albumCm: AlbumCardModel,
  onClose: () => void
}){
  const width = 260;
  const horizontalLocation = (window.innerWidth / 2) - (width / 2);

  const [isDragging, setIsDragging] = useState(false);
  const [position, setPosition] = useState({ x: horizontalLocation, y: 0 });
  const [offset, setOffset] = useState({ x: 0, y: 0 });
  
  const mouseHandler = {
    down: (e: any) => {
      setIsDragging(true);
      setOffset({
        x: e.clientX - position.x,
        y: e.clientY - position.y
      });
    },
    move: (e: any) => {
      if (!isDragging) return;
      setPosition({
        x: e.clientX - offset.x,
        y: e.clientY - offset.y
      });
    },
    up: () => {
      setIsDragging(false);
    }
  }

  const uri = _uri.GetAlbumFsNodeInfo(0, props.albumCm.path, true, true);

  const { data, error } = useSWR<AlbumFsNodeInfo, any>(uri);

  return(
    <div style={{
      position: 'absolute',
      width: `${width}px`,
      borderRadius: '10px',
      boxShadow: `0px 0px 20px ${cssVariables.highlightLight}`, 
      padding: '10px',
      backgroundColor: cssVariables.bgL3,
      transform: `translate(${position.x}px, ${position.y}px)`,
      zIndex: 1,
    }}
      onMouseDown={mouseHandler.down}
      onMouseMove={mouseHandler.move}
      onMouseUp={mouseHandler.up}
    >
      <div style={{display:'flex'}}>
        <Typography style={{flex:'1'}}>{props.albumCm.path}</Typography>
        <div
          style={{cursor:'pointer', paddingLeft:'4px', paddingRight:'4px'}} 
          onClick={props.onClose}
        >
          <Typography>X</Typography>
        </div>
      </div>
      <div className='divider-8'></div>
      {error ? <Typography>{JSON.stringify(error)}</Typography> :
      !data ? <Typography>Loading...</Typography> :
        <DownloadModal 
          fsNodes={data.fsNodes}
          albumCm={props.albumCm}
          type={0}
        />
      }
    </div>
  )
}

export default function DownloadModal(props: {
  fsNodes: FsNode[],
  albumCm: AlbumCardModel,
  type: 0 | 1
}){
  const albumPath = props.albumCm.path;

  const [fileDownloads, setFileDownloads] = useState<FileDownload[]>(
    _helper.getFlatFilesFsNodes(props.fsNodes)
      .map(a => MapFsNodeToFileDownload(a, albumPath))
  );

  const { axiosE } = useAuth();

  const performSequentialFileDownload = async () => {
    const pendingFiles = fileDownloads.filter(a => a.downloadState === 'pending');
    for (let i = 0; i < pendingFiles.length; i++) {
      const nextDownload = pendingFiles[i];

      setFileDownloads(prev => {
        const newDownloads = [...prev];
        newDownloads.find(a => a.id === nextDownload.id)!.downloadState = 'downloading';
        return newDownloads;
      });

      const url = _uri.StreamPage(`${albumPath}\\${nextDownload.alRelPath}`, props.type);

      const response = await axiosE.get(url, {
        responseType: 'blob', // Tell Axios to expect a blob response
      });


      if(response.status !== 200){
        setFileDownloads(prev => {
          const newDownloads = [...prev];
          newDownloads.find(a => a.id === nextDownload.id)!.downloadState = 'error';
          newDownloads.find(a => a.id === nextDownload.id)!.errorMessage = response.statusText;
          return newDownloads;
        });
        continue;
      }

      const blob = response.data;

      //check if file already exists
      const existingFile = await _db.offlineFiles.get(nextDownload.id);
      if(existingFile){
        //delete existing file
        await _db.offlineFiles.delete(nextDownload.id);
      }

      await _db.offlineFiles.add({
        id: nextDownload.id,
        albumPath: albumPath,
        alRelPath: nextDownload.alRelPath,
        blob: blob
      });

      setFileDownloads(prev => {
        const newDownloads = [...prev];
        newDownloads.find(a => a.id === nextDownload.id)!.downloadState = 'finished';
        return newDownloads;
      });
    }

    const existingAlbumVm = await _db.albumCms.get(albumPath);
    if(existingAlbumVm){
      await _db.albumCms.delete(albumPath);
    }

    await _db.albumCms.add({
      ...props.albumCm,
      fsNodes: props.fsNodes
    });
  };

  useEffect(() => {
    performSequentialFileDownload();
  }, []);

  return (
    <div style={{height:'200px', overflowY:'scroll'}}>
      {fileDownloads.map(a => 
        (<FileDownloadDisplay key={a.id} file={a} />)
      )}
    </div>
  );
}

function FileDownloadDisplay(props: {file: FileDownload}){
  return (
    <div style={{display:'flex'}}>
      <div style={{flex:'1'}}>
        <Typography>{ _helper.limitStringToNCharsWithLeadingPad(props.file.alRelPath, 26, '...')}</Typography>
      </div>
      <div>
        <Typography>
          <div style={{paddingLeft:'4px', paddingRight:'4px'}}>
          {props.file.downloadState === 'pending'
            ? <HourglassOutlined /> 
            : props.file.downloadState === 'error'
            ? <Tooltip title={props.file.errorMessage}>
                <CloseOutlined />
              </Tooltip>
            : props.file.downloadState === 'downloading' 
            ? <SyncOutlined spin /> 
            : <CheckOutlined />
          }
          </div>
        </Typography>
      </div>
    </div>
  );
}