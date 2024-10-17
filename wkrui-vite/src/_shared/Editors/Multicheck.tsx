import { Button } from 'antd';
import { Row, Col } from 'antd';

export default function Multicheck(props : {
  value?: string[],
  items: string[],
  onChange?: (value: string[]) => void
}) {
  function handleClick(val: string){
    if(!props.value || !props.onChange) return;

    const newVal = props.value?.includes(val) ? props.value.filter(a => a !== val) : [...props.value, val];
    props.onChange(newVal);
  }
  
  return (
    <Row gutter={2}>
      {props.items.map((item) => (
        <Col span={6} key={item}>
          <Button
            key={item}
            type={props.value?.includes(item) ? "primary" : "link"}
            ghost={props.value?.includes(item)}
            onClick={() => { handleClick(item); }}
            className="w-full mb-2 justify-start"
          >
            {item}
          </Button>
        </Col>)
      )}
    </Row>
  );
}