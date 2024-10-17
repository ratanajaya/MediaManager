import { Rate } from 'antd';
import { Row, Col } from 'antd';

export default function MyInputStar(props : {
  colSpan? : {
    label: number,
    control: number
  },
  label?: string,
  value: number,
  onChange: (label: string, value: number) => void
}) {
  const colSpan = props.colSpan ?? { label: 5, control: 19 };

  return (
    <>
      <Row gutter={[0, 8]}>
        <Col span={colSpan.label}>
          <label className='my-editor-label'>{props.label}</label>
        </Col>
        <Col span={colSpan.control}>
          <Rate
            count={3}
            value={props.value}
            onChange={(value) => { props.onChange(props.label ?? "", value); } }
          />
        </Col>
      </Row>
    </>
  );
}