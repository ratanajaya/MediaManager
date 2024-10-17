import { useState, useEffect } from 'react';
import { Modal, Button, Input } from 'antd';

export default function TextDialog(props: {
  visible: boolean,
  btnText?: string | null,
  initialValue: string,
  onClose: () => void,
  onOk: (newVal: string, initialVal: string) => void,
}) {
  const [value, setValue] = useState("");
  useEffect(() => { setValue(props.initialValue) }, [props.initialValue]);

  return (
    <Modal
      open={props.visible}
      footer={null}
      centered={true}
      closable={false}
      onCancel={props.onClose}
      style={{ maxWidth: 300, textAlign: "center" }}
    >
      <Input.TextArea value={value} onChange={(event) => setValue(event.target.value)} rows={2} />
      <Button
        style={{ width: "100%" }} type="primary"
        onClick={() => { props.onOk(value, props.initialValue); props.onClose(); }}
      >
        {props.btnText ?? 'Rename'}
      </Button>
    </Modal>
  );
}