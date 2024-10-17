import { notification } from 'antd';
import { ArgsProps } from 'antd/es/notification';
import { createContext, useContext } from 'react';

interface NotifContextInterface {
  notif: {
    info: (title: string, stringContent: string, objContent: any) => void;
    error: (title: string, stringContent: string, objContent?: any) => void;
    apiError: (error: any) => void;
  },
  contextHolder: any;
}

//@ts-ignore
const NotifContext = createContext<NotifContextInterface>();

const useNotification = () => useContext(NotifContext);

export default useNotification;

export const NotifProvider = ({ children }: { children: React.ReactNode }) => {
  const [api, contextHolder] = notification.useNotification();
  function info(title: string, stringContent: string, objContent: any) {
    api.info({
      message: (title),
      description: (
        <>
          <span>{stringContent}</span>
          <pre>{objContent ? JSON.stringify(objContent, null, 2) : ""}</pre>
        </>
      ),
      duration: 3
    });
  }

  function error(title: string, stringContent: string, objContent?: any) {
    api.error({
      message: (title),
      description: (
        <>
          <span>{stringContent}</span>
          <pre>{objContent ? JSON.stringify(objContent, null, 2) : ""}</pre>
        </>
      ),
      duration: 3
    });
  }

  function apiError(error: any) {
    if (error.response === null || error.response === undefined) {
      api.error({
        message: ('No response'),
        description: (error.toString()),
        duration: 3
      });

      return;
    }
    const { status, statusText, data } = error.response;

    const content: ArgsProps = {
      message: `${status} - ${statusText}`,
      description: (
        <>
          <span>{JSON.stringify(data, null, 2)}</span>
        </>
      ),
      duration: 3
    };

    const notifier = status >= 400 && status < 500 ? api.warning : api.error;
    notifier(content);
  }

  const notif = {
    info,
    error,
    apiError
  }
  
  return (
    <NotifContext.Provider value={{ notif, contextHolder }}>
      {contextHolder}
      {children}
    </NotifContext.Provider>
  );
}
