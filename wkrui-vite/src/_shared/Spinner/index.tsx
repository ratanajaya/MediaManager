import { Spin } from 'antd'
import React, { ReactNode } from 'react'
import cssVariables from '_assets/styles/cssVariables';
import { LoadingOutlined } from '@ant-design/icons';

export default function Spinner(props: {
  loading: boolean
}) {
  return (
    <div style={{
      position:'fixed',
      height: '100%',
      width: '100%',
      top: 0,
      right: 0,
      zIndex: 999,
      background: cssVariables.overlayMain,
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'center',
    }}>
      <Spin size='large' spinning={props.loading} indicator={<LoadingOutlined style={{fontSize: 128}} />} />
    </div>
  );
}

export function LocalSpinner(props:{
  children: ReactNode,
  loading: boolean
}) {
  return (
    <Spin spinning={props.loading ?? false}
        indicator={(<LoadingOutlined style={{ fontSize: 40 }} spin />)}
      >
      {props.children}
    </Spin>
  )
}