import { Button, Progress, Typography, Space } from 'antd';

import _constant from "_utils/_constant";
import { EventStreamData } from '_utils/Types';

export default function ActionButton(props: ActionButtonType){
  return(
    <>
      <div style={{display:"flex", flexDirection:"row", marginBottom:"5px"}}>
        <div style={{width:"130px"}}>
        <Button
          type='primary' ghost
          icon={props.icon}
          loading={props.isLoading}
          onClick={props.execute ?? (() =>{}) }
          block={true}
          style={{ width:"100%", maxWidth:"140px"}}
          disabled={_constant.isPublic}
        >
          {props.text}
        </Button>
        </div>
        <div style={{
          height:"32px", 
          flex:"auto", 
          marginLeft:"10px",
          display:"flex",
          justifyContent: "flex-end",
          flexDirection: "column"
        }}>
          <Progress strokeLinecap="square" percent={props.percent} />
        </div>
      </div>
      <div hidden={props.eventMsgs.length === 0}
        style={{display:"flex", flexDirection:"row", marginBottom:"5px"}}>
        <div style={{width:"130px"}}></div>
        <div style={{
          flex:"auto",
          marginLeft:"10px",
          paddingRight:"32px"
        }}>
          <Space direction="vertical">
            {props.eventMsgs.map((e, i) => <Typography.Text key={i}>{e.message}</Typography.Text>)}
          </Space>
        </div>
      </div>
    </>
  );
}

export type ActionButtonType = {
  icon: JSX.Element,
  isLoading: boolean,
  text: string,
  eventMsgs: EventStreamData[],
  percent: number,
  progress?: number,
  execute: ((event: any) => void) | null
}