import React, { useState, useEffect, ReactNode } from 'react';

import { Button, Input, Space, Switch, Typography } from 'antd';

import * as CSS from 'csstype';
import _constant from "_utils/_constant";
import _uri from "_utils/_uri";
import { ClockCircleOutlined, PoweroffOutlined } from '@ant-design/icons';
import { useSetting } from '_shared/Contexts/SettingProvider';
import { useAuth } from '_shared/Contexts/useAuth';
import useNotification from '_shared/Contexts/NotifProvider';
import { LocalSpinner } from '_shared/Spinner';
import { ResponseResult } from '_utils/Types';

export default function SettingForm(){
  const { axiosA } = useAuth();
  const { setting, setSetting, saveSetting } = useSetting();
  const [useCensorship, setUseCenshorship] = useState<boolean | null>(null);
  const [imageProcessorOn, setImageProcessorOn] = useState<boolean>(false);
  const [loading, setLoading] = useState<boolean>(false);

  const { notif } = useNotification();

  useEffect(() => {
    setLoading(true);
    Promise.all([
      axiosA.get<boolean>(_uri.Censorship())
        .then((response) => {
          setUseCenshorship(response.data)
        })
        .catch((error) => {
          notif.apiError(error);
        }),

      axiosA.get<ResponseResult<boolean>>(_uri.IpCheck())
        .then((response) => {
          setImageProcessorOn(response.data.success);
          
          if(!response.data.success)
            notif.error(response.data.message, '', response.data);
        })
        .catch((error) => {
          notif.apiError(error);
        }),
    ])
    .finally(() => setLoading(false));
  },[]);

  function censorshipChanged(checked: boolean){
    axiosA.post(_uri.Censorship(), { }, { params:{ status:checked } })
      .then((response) => {
        setUseCenshorship(checked)
      })
      .catch((error) => {
        notif.apiError(error);
      });
  }

  function pcAction(action: string){
    const url = action === 'Sleep' ? _uri.Sleep() : _uri.Hibernate();

    axiosA.post(url)
      .then(res => {
        notif.info(`${action} command sent`, '', {});
      })
      .catch(err => {
        notif.apiError(err);
      });
  }

  function ipPcAction(action: string){
    const url = action === 'Sleep' ? _uri.IpSleep() : _uri.IpShutdown();

    axiosA.post(url)
      .then(res => {
        notif.info(`${action} command sent`, '', {});
      })
      .catch(err => {
        notif.apiError(err);
      });
  }

  return(
    <LocalSpinner loading={loading}>
      <div className=' flex gap-3 mb-3 mt-1'>
        <Space>
          <Button
            danger
            className=' h-16 w-16'
            onClick={() => { pcAction('Sleep'); }}
          >
            <ClockCircleOutlined style={{fontSize:36}} />
          </Button>
          <Button
            danger
            className=' h-16 w-16'
            onClick={() => { pcAction('Hibernate'); }}
          >
            <PoweroffOutlined style={{fontSize:36}} />
          </Button>
        </Space>
        <Space className=' flex-1'>
          <Button
            className=' h-16 w-16'
            onClick={() => { ipPcAction('Sleep'); }}
            disabled={!imageProcessorOn}
          >
            <ClockCircleOutlined style={{fontSize:36}} />
          </Button>
          <Button
            className=' h-16 w-16'
            onClick={() => { ipPcAction('Shutdown'); }}
            disabled={!imageProcessorOn}
          >
            <PoweroffOutlined style={{fontSize:36}} />
          </Button>

        </Space>
      </div>
      <FormRow
        col0={
          <Typography.Text strong>API</Typography.Text>
        }
        col1={
          <Input
            value={setting.apiBaseUrl} 
            disabled={true}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Media</Typography.Text>
        }
        col1={
          <Input 
            value={setting.mediaBaseUrl} 
            onChange={(e) => {
              setSetting(prev => {
                return {
                  ...prev,
                  mediaBaseUrl: e.target.value
                };
              });
            }}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Res Media</Typography.Text>
        }
        col1={
          <Input 
            value={setting.resMediaBaseUrl} 
            onChange={(e) => { 
              setSetting(prev => {
                return {
                  ...prev,
                  resMediaBaseUrl: e.target.value
                };
              });
            }}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Always Portrait</Typography.Text>
        }
        col1={
          <Switch
            checked={setting.alwaysPortrait}
            onChange={(val) => {
              setSetting(prev => {
                return {
                  ...prev,
                  alwaysPortrait: val
                };
              });
            }}
            style={{width:"40px"}}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Mute Video</Typography.Text>
        }
        col1={
          <Switch
            checked={setting.muteVideo}
            onChange={(val) => {
              setSetting(prev => {
                return {
                  ...prev,
                  muteVideo: val
                };
              });
            }}
            style={{width:"40px"}}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Direct File Access</Typography.Text>
        }
        col1={
          <Switch
            checked={setting.directFileAccess}
            onChange={(val) => {
              setSetting(prev => {
                return {
                  ...prev,
                  directFileAccess: val
                };
              });
            }}
            style={{width:"40px"}}
          />
        }
      />
      <FormRow
        col0={
          <Typography.Text strong>Censorship</Typography.Text>
        }
        col1={
          <Switch
            checked={useCensorship ?? false}
            onChange={censorshipChanged}
            style={{width:"40px"}}
          />
        }
      />
      <FormRow
        col1={ 
          <Button
            type='primary' ghost
            onClick={() => {
              saveSetting();
            }}
            style={{ width:"100%", maxWidth:"100px"}}
            disabled={_constant.isPublic}
          >
            Save
          </Button>
        }
      />
    </LocalSpinner>
  );
}

function FormRow(props: {
  col0?: ReactNode,
  col1?: ReactNode,
}){
  const style: CSS.Properties = {
    display:"flex", 
    flexDirection:"row", 
    marginBottom:"5px",
    textAlign: "right"
  };

  return(
    <div style={style}>
      <div style={{width:"130px"}}>
        {props.col0}
      </div>
      <div style={{
        height:"32px",
        width:"100%",
        textAlign:"left",
        marginLeft:"10px",
      }}>
        {props.col1}
      </div>
    </div>
  );
}