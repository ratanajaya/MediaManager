import { useState, useEffect } from 'react';

import { Modal, Button, Rate, Input, Space } from 'antd';
import { ArrowLeftOutlined, ArrowRightOutlined, DeleteOutlined, EditOutlined, FileSyncOutlined, SwapOutlined } from '@ant-design/icons';

import TextDialog from '_shared/Modals/TextDialog';
import DirNodeDialog from '_shared/Modals/DirNodeDialog';
import { AlbumCardModel } from '_utils/Types';

export default function ReaderModalContextMenu(props:{
  albumCm: AlbumCardModel,
  pageName: string,
  initialValue: number,
  visible: boolean,
  type: 0 | 1,
  onJump: (val: number) => void,
  onUndoJump: () => void,
  onTierChange: (val: number) => void,
  onRecount: () => void,
  onRename: (initialVal: string, newVal: string) => void,
  onMove: (selected?: string) => void,
  onDelete: () => void,
  onCommitDelete: () => void,
  onCancel: () => void,
  onJumpToLast: () => void,
  onArtistClick: (artist: string) => void
}) {
  const [jumpValue, setJumpValue] = useState(0);
  useEffect(() => {
    setJumpValue(props.initialValue + 1);
  }, [props.initialValue]);

  const changeJumpValue = (val: number) => {
    setJumpValue(Math.min(Math.max(val, 1), props.albumCm.pageCount));
  };

  const [showRenameModal, setShowRenameModal] = useState(false);
  const [showDirNodeModal, setShowDirNodeModal] = useState(false);

  const [tierDisplay, setTierDisplay] = useState(0);
  useEffect(() => {
    if (props.albumCm === undefined) { return; }
    setTierDisplay(props.albumCm.tier);
  }, [props.albumCm]);

  if (props.albumCm === undefined) { return (<></>); }

  return (
    <>
      <Modal
        open={props.visible}
        onCancel={props.onCancel}
        footer={null}
        closable={false}
        centered={true}
        style={{ maxWidth: 300 }}
      >
        <Space direction='vertical'>
          {props.type !== 1 && 
            <div style={{ textAlign: "center" }}>
              <Rate
                count={3}
                value={tierDisplay}
                onChange={(value) => { setTierDisplay(value); props.onTierChange(value); }}
                style={{ fontSize: "40px" }}
              />
            </div>
          }
          {props.type === 0 && 
            <div className=' text-center'>
              {props.albumCm.artistDisplay.split(',').map(a => a.trim()).map((a, i) => (
                <Button key={i} type="link" onClick={() => props.onArtistClick(a)}>
                  {a}
                </Button>
              ))}
            </div>
          }
          <div style={{ display:'flex', gap:'4px' }}>
            <Input type="number" style={{ flex:'2' }} 
              value={jumpValue} onChange={(e) => changeJumpValue(parseInt(e.target.value))} 
            />
            <Button style={{ flex:'1' }}
              onClick={() => changeJumpValue(jumpValue - 1)}>
              -
            </Button>
            <Button style={{ flex:'1' }}
              onClick={() => changeJumpValue(jumpValue + 1)}>
              +
            </Button>
          </div>
          <div>
            <Button type="primary" style={{ width: "100%" }}
              onClick={() => props.onJump(jumpValue - 1)}>
              <ArrowRightOutlined />Jump
            </Button>
          </div>
          <div>
            <Button type="primary" style={{ width: "100%" }}
              onClick={() => props.onJumpToLast()}>
              <ArrowRightOutlined />Jump to Last
            </Button>
          </div>
          <div>
            <Button type="primary" style={{ width: "100%" }}
              onClick={() => props.onUndoJump()}>
              <ArrowLeftOutlined />Undo Jump
            </Button>
          </div>
          <div>
            <Button type="primary" style={{ width: "100%" }}
              onClick={() => props.onRecount()}>
              <FileSyncOutlined />Recount Pages
            </Button>
          </div>
          <div>
            <Button type="primary" style={{ width: "100%" }}
              onClick={() => { props.onCancel(); setShowRenameModal(true); }}>
              <EditOutlined />Rename Page
            </Button>
          </div>
          {props.type === 1 && 
            <div>
              <Button type="primary" style={{ width: "100%" }}
                onClick={() => { props.onCancel(); setShowDirNodeModal(true); }}>
                <SwapOutlined />Move Page
              </Button>
            </div>
          }
          <div>
            <Button type="primary" danger style={{ width: "100%" }}
              onContextMenu={(e) => { e.preventDefault(); props.onCancel(); props.onCommitDelete(); }}
            >
              <DeleteOutlined />Delete Page
            </Button>
          </div>
        </Space>
      </Modal>
      <TextDialog
        visible={showRenameModal} onClose={() => setShowRenameModal(false)}
        initialValue={props.pageName}
        onOk={props.onRename}
      />
      {(props.type === 1 && showDirNodeModal) &&
        <DirNodeDialog
          open={showDirNodeModal} 
          onClose={() => setShowDirNodeModal(false)}
          onOk={props.onMove}
        />
      }
    </>
  )
}