import { useState, useEffect } from 'react';

import { Button, Checkbox, Modal, Form, Input, Radio, Rate, Collapse, Typography } from 'antd';

import _helper from '_utils/_helper';
import _uri from '_utils/_uri';
import { Album, AlbumVM } from '_utils/Types';
import Multicheck from '_shared/Editors/Multicheck';
import InputButton from '_shared/Editors/InputButton';
import Multitag from '_shared/Editors/Multitag';
import { CaretRightFilled, DeleteOutlined, DownloadOutlined, ReloadOutlined, ToolOutlined } from '@ant-design/icons';
import { useAlbumInfo } from '_shared/Contexts/AlbumInfoProvider';
import { useAuth } from '_shared/Contexts/useAuth';
import CommentPanel from './CommentPanel';
import useNotification from '_shared/Contexts/NotifProvider';

const { Item, useForm } = Form;

export default function EditModal(props: {
  type: 0 | 1,
  albumPath: string | null,
  onOk: (albumVm: AlbumVM) => void,
  onEditFiles: (albumVm: AlbumVM) => void,
  onCorrectFiles: (albumVm: AlbumVM) => void,
  onRefresh: (albumVm: AlbumVM) => void,
  onDelete: (albumVm: AlbumVM) => void,
  onDownload: (albumVm: AlbumVM) => void,
  onCancel: () => void,
}) {
  const { axiosA } = useAuth();
  const albumInfo = useAlbumInfo();
  const { notif } = useNotification();

  const [albumVm, setAlbumVm] = useState<AlbumVM>({
    path: "", //not updated
    pageCount: 0, //not updated
    lastPageIndex: 0, //not updated
    album: {
      title: "",
      category: "Manga",
      orientation: "Portrait",
      artists: [],
      tags: [],
      povs: [],
      focuses: [],
      others: [],
      rares: [],
      qualities: [],
      characters: [],
      languages: [],
      sources: [],
      note: "",
      tier: 0,
      isWip: false,
      isRead: false,
      entryDate: null
    }
  });

  const [form] = useForm();
  const album = albumVm.album;
  useEffect(() => {
    form.resetFields();
  },[albumVm]);

  useEffect(() => {
    if(props.albumPath === null){
      form.resetFields();

      return;
    }
    axiosA.get<AlbumVM>(_uri.GetAlbumVm(props.type, props.albumPath))
      .then(function (response) {
        setAlbumVm(response.data);
      })
      .catch(function (error: any) {
        notif.apiError(error);
      });

  }, [props.albumPath]);

  const handlers = {
    albumVmChange: function (label: string, value: any) {
      const newAlbumVm = { ...albumVm };
      let cleanedValue: any = value;
      if (label === "Artists") {
        cleanedValue = value.split(",");
      }
      else if (label === "Povs" || label === "Focuses" || label === "Others" || label === "Rares" || label === "Qualities" || 
        label === "Languages" || label === "Characters") {

        cleanedValue = albumVm.album[_helper.firstLetterLowerCase(label) as keyof Album];
        if (cleanedValue.includes(value)) {
          cleanedValue = cleanedValue.filter((a: string) => a !== value);
        }
        else {
          cleanedValue.push(value);
        }
      }
      //@ts-ignore
      newAlbumVm.album[_helper.firstLetterLowerCase(label) as keyof Album] = cleanedValue;
      setAlbumVm(newAlbumVm);
    },
    ok: function () {
      const formVal = form.getFieldsValue();
      const newAlbumVm: AlbumVM = {
        ...albumVm,
        album: {
          ...albumVm.album,
          ...formVal,
          artists: formVal.artists.split(','),
          isWip: formVal.flags.includes('isWip'),
          isRead: formVal.flags.includes('isRead'),
        }
      }

      props.onOk(newAlbumVm);
    },
  }

  const [activeKeys, setActiveKeys] = useState<string | string[]>(['panelMetadata']);

  if(props.albumPath == null) return <></>;

  return(
    <Modal 
      open={true}
      onCancel={props.onCancel}
      closable={false}
      width={580}
      footer={
        <div style={{width:'100%', display:'flex', gap:'10px'}}>
          <div style={{flex:'3', display:'flex', gap:'10px'}}>
            <div style={{flex:'1'}}>
              <Button 
                danger
                style={{width:'100%'}}
                onClick={() => props.onDelete(albumVm)} 
              >
                <DeleteOutlined />
              </Button>
            </div>
            <div style={{flex:'1'}}>
              <a 
                href={`/#/ScCorrection?path=${encodeURIComponent(props.albumPath)}`}
                style={{
                  width: '100%', 
                  display: 'flex', 
                  justifyContent: 'center', // This is for horizontal centering
                  alignItems: 'center',     // This is for vertical centering
                  height: '100%'            // Take the full height of the parent
                }}
              >
                C
                <ToolOutlined />
              </a>
            </div>
            <div style={{flex:'1'}}>
              <Button 
                style={{width:'100%'}}
                onClick={() => props.onRefresh(albumVm)} 
              >
                <ReloadOutlined />
              </Button>
            </div>
            <div style={{flex:'1'}}>
              <Button 
                style={{width:'100%'}}
                onClick={() => props.onDownload(albumVm)} 
              >
                <DownloadOutlined />
              </Button>
            </div>
          </div>
          <div style={{flex:'1'}}>
            <Button 
              type='primary' ghost
              style={{width:'100%'}}
              onClick={handlers.ok} 
            >
              Save
            </Button>
          </div>
        </div>
      }
    >
      {!albumInfo || albumVm.path === '' ? "loading..." : (
        <Collapse 
          bordered={false}
          expandIcon={({ isActive }) => <CaretRightFilled rotate={isActive ? 90 : 0} />}
          style={{marginBottom:'10px'}}
          accordion
          activeKey={activeKeys}
          onChange={setActiveKeys}
        >
          <Collapse.Panel className='editPanel' header='Comments' key='panelComment'>
            <CommentPanel 
              open={activeKeys === 'panelComment'}
              albumVm={albumVm}
              setAlbumVM={setAlbumVm}
            />
          </Collapse.Panel>
          <Collapse.Panel className='editPanel' header='Metadata' key='panelMetadata'>
            <Form
              form={form}
              layout='horizontal'
              size='middle'
              colon={false}
              labelCol={{
                span: 4,
              }}
              wrapperCol={{
                span: 20,
              }}
              initialValues={{
                'title': album.title,
                'artists': album.artists.join(','),
                'category': album.category,
                'orientation': album.orientation,
                'tags': album.tags,
                'povs': album.povs,
                'focuses': album.focuses,
                'others': album.others,
                'rares': album.rares,
                'qualities': album.qualities,
                'languages': album.languages,
                'characters': album.characters,
                'note': album.note,
                'tier': album.tier,
                'flags': [
                  ...(album.isWip ? ['isWip'] : []), 
                  ...(album.isRead ? ['isRead'] : [])
                ]
              }}
            >
              <Item label="Title" name="title">
                <Input.TextArea rows={3} />
              </Item>
              <Item label="Artists" name="artists">
                <Input />
              </Item>
              <Item label="Category" name="category">
                <Radio.Group>
                  {albumInfo.categories.map((a, i) => (
                    <Radio key={a} value={a} style={{width:'100px'}}>{a}</Radio>
                  ))}
                </Radio.Group>
              </Item>
              <Item label="Orientation" name="orientation">
                <Radio.Group>
                  {albumInfo.orientations.map((a, i) => (
                    <Radio key={a} value={a} style={{width:'100px'}}>{a}</Radio>
                  ))}
                </Radio.Group>
              </Item>
              <Item label="Povs" name="povs">
                <Multicheck items={albumInfo.povs} />
              </Item>
              <Item label="Focuses" name="focuses">
                <Multicheck items={albumInfo.focuses} />
              </Item>
              <Item label="Others" name="others">
                <Multicheck items={albumInfo.others} />
              </Item>
              <Item label="Rares" name="rares">
                <Multicheck items={albumInfo.rares} />
              </Item>
              <Item label="Qualities" name="qualities">
                <Multicheck items={albumInfo.qualities} />
              </Item>
              <Item label="Languages" name="languages">
                <Multicheck items={albumInfo.languages} />
              </Item>
              <Item label="Characters" name="characters">
                <Multitag items={albumInfo.characters} />
              </Item>
              <Item label="Note" name="note">
                <InputButton onClick={() => { form.setFieldValue('note', ''); }} />
              </Item>
              <Item label="Tier" name="tier">
                <Rate count={3} />
              </Item>
              <Item label="Flags" name="flags">
                <Checkbox.Group>
                  <Checkbox value="isWip" style={{width:'100px'}}>WIP</Checkbox>
                  <Checkbox value="isRead" style={{width:'100px'}}>Read</Checkbox>
                </Checkbox.Group>
              </Item>
            </Form>
            <div style={{ fontSize:'0.8em' }}>
              {albumVm.path.split('\\').pop()!}
            </div>
          </Collapse.Panel>
        </Collapse>
      )}
    </Modal>
  );
}