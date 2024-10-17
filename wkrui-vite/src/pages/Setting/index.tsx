import { useState  } from 'react';

import {  Divider } from 'antd';

import _uri from "_utils/_uri";
import _helper from "_utils/_helper";
import { CheckCircleOutlined, ReloadOutlined, SyncOutlined } from '@ant-design/icons';
import ActionButton, { ActionButtonType } from 'pages/Setting/ActionButton';
import SettingForm from 'pages/Setting/SettingForm';
import useNotification from '_shared/Contexts/NotifProvider';

export default function Setting(){
  const { notif } = useNotification();

  function setLoading(btnName: string, isLoading: boolean){
    setButton((prev) => {
      prev[btnName].isLoading = isLoading;
      prev[btnName].progress = isLoading ? 0 : 100;
      return {
        ...prev
      }
    });
  }

  function setDefaultButton(text: string, icon: JSX.Element, name: string, uri: string):ActionButtonType {
    return {
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: text,
      icon: icon,
      execute: () => {
        setLoading(name, true);

        const sse = new EventSource(uri, {});
        sse.onmessage = (res) => {
          const resData = JSON.parse(res.data);
          const isLoading = resData.currentStep < resData.maxStep;
          if(!isLoading) sse.close();
          if(resData.isError) 
            notif.error("API Error", resData.message);

          const percent = _helper.getPercent100(resData.currentStep, resData.maxStep);
          setButton(prev => { 
            prev[name].percent = percent;
            prev[name].isLoading = isLoading;
            prev[name].eventMsgs.push(resData);
            return {
              ...prev
            };
          });
        }
      }
    }
  }
  
  const [button, setButton] = useState<{[key: string]: ActionButtonType}>({
    reload: setDefaultButton('Reload', <ReloadOutlined />,'reload', _uri.ReloadDatabase()),
    rescan: setDefaultButton('Rescan', <SyncOutlined />, 'rescan', _uri.RescanDatabase()),
    // testSSE: setDefaultButton('Test SSE',<SyncOutlined />, 'testSSE', 'http://localhost:51474/sse/testsse'),
    checkApiUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
    checkMediaUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
    checkResMediaUrl:{
      isLoading: false,
      percent: 0,
      eventMsgs: [],
      text: 'Check Url',
      icon: <CheckCircleOutlined />,
      execute: null
    },
  });

  return (
    <>
      <SettingForm />

      <Divider orientation="left">
        Actions
      </Divider>
      <ActionButton {...button.reload} />
      <ActionButton {...button.rescan} />
      {/* <ActionButton {...button.testSSE} /> */}

      <Divider orientation="left">
        Event Stream
      </Divider>
    </>
  );
}