import { Modal } from "antd";
import { createContext, useContext } from "react";

import { ModalFuncProps } from 'antd/lib/modal';

interface ModalContextInterface {
  modal: {
    info: (config: ModalFuncProps) => void;
    success: (config: ModalFuncProps) => void;
    error: (config: ModalFuncProps) => void;
    warning: (config: ModalFuncProps) => void;
    confirm: (config: ModalFuncProps) => void;
  };
  contextHolder: any;
}

//@ts-ignore
const ModalContext = createContext<ModalContextInterface>();

export const useModal = () => useContext(ModalContext);

export const ModalProvider = ({ children }: { children: React.ReactNode }) => {
  const [modal, contextHolder] = Modal.useModal();

  return (
    <ModalContext.Provider value={{ modal, contextHolder }}>
      {contextHolder}
      {children}
    </ModalContext.Provider>
  );
};