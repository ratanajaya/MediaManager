import { Button, Input, Layout } from 'antd';
import { useState } from 'react';
import PatternLock from 'react-pattern-lock';
import cssVariables from '_assets/styles/cssVariables';
import { useAuth } from '_shared/Contexts/useAuth';
import _uri from '_utils/_uri';
import { LocalSpinner } from '_shared/Spinner';
import _ls from '_utils/_ls';
import _constant from '_utils/_constant';
import { useSetting } from '_shared/Contexts/SettingProvider';
import useNotification from '_shared/Contexts/NotifProvider';
import { useNavigate } from 'react-router-dom';

export default function Login() {
  const { axiosE, setToken } = useAuth();
  const { notif } = useNotification();
  const {setting, setSetting, saveSetting} = useSetting();
  const navigate = useNavigate();

  const [ loading, setLoading ] = useState<boolean>(false);
  const [ pattern, setPattern ] = useState<number[]>([]);

  function checkSaveUrl(url: string){
    axiosE.get<any>(url)
      .then((response) => {
        notif.info(`${response.status} ${response.statusText}`, '', response.data);
        saveSetting();
      })
      .catch((error) => {
        notif.apiError(error);
      });
  }

  function login() {
    setLoading(true);

    axiosE.post<string>(_uri.Login(), { }, {
        params: {
          password: pattern.join('')
        }
      })
      .then(res => {
        _ls.set(_constant.lsKey.authToken, res.data);
        setToken(res.data);
        navigate('/Setting');
      })
      .catch(err => {
        notif.apiError(err);
        setPattern([]);
        setLoading(false);
      });
  }

  return (
    <Layout style={{ 
      width:'100%', 
      height:'100vh',
      display: 'flex',
      justifyContent: 'center',
      alignItems: 'center',
      flexDirection: 'column'
    }}>
      <div style={{
        width: '400px',
        maxWidth: '100vw',
        padding: '16px',
        backgroundColor: cssVariables.bgL2,
        borderRadius: '8px'
      }}>
        <LocalSpinner loading={loading}>
          <PatternLock
            width='100%'
            pointSize={ 15 }
            size={ 3 }
            path={ pattern }
            onChange={ (val) => {
              setPattern(val);
            }}
            onFinish={login}
          />
          {/* <Button onClick={login}>Login</Button> */}
          <Input.Search 
            value={setting.apiBaseUrl} 
            onChange={(e) => { 
              setSetting(prev => {
                return {
                  ...prev,
                  apiBaseUrl: e.target.value
                };
              });
            }} 
            onSearch={() => { 
              checkSaveUrl(setting.apiBaseUrl ?? ''); 
            }}
          />
        </LocalSpinner>
      </div>
    </Layout>
  )
}