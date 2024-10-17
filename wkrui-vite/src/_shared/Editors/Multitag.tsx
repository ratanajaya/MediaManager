import { Popover, Tag } from 'antd';
import { PlusOutlined } from '@ant-design/icons';

export default function Multitag(props : {
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
    <div>
      {props.value?.map((item) => (
        <Tag key={item} closable={true} onClose={() => handleClick(item)}>
          {item}
        </Tag>
      ))}
      <Popover placement="topLeft" trigger="click" 
        content={
          <>
            {props.items.filter((item) => { return !props.value?.includes(item); })
              .map((item, index) => (
                <div key={index}>
                  <a href='/' onClick={(e) => { e.preventDefault(); handleClick(item)}}>
                    {item}
                  </a>
                </div>
              )
            )}
          </>
        }
      >
        <Tag style={{ borderStyle: 'dashed', cursor: 'pointer' }}>
          <PlusOutlined /> Add
        </Tag>
      </Popover>
    </div>
  );
}