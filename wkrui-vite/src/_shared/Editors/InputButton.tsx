import { Button, Input, Space } from 'antd';
import { CloseOutlined } from '@ant-design/icons';

export default function InputButton(props : {
  value?: string[],
  onChange?: (value: string) => void,
  onClick?: () => void
}) {  
  return (
    <Space.Compact style={{display:'flex'}}>
      <Input value={props.value} onChange={(val) => { props.onChange?.(val.target.value) }} style={{flex:'1'}} />
      <Button  onClick={props.onClick}><CloseOutlined /></Button>
    </Space.Compact>
  );
}