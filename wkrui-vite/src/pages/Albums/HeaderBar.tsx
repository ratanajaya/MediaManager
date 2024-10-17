import React, { useState, useEffect } from 'react';

import { Row, Col, Collapse, Switch, Button, Checkbox, Radio, Select, Spin } from 'antd';
import { CaretRightFilled } from '@ant-design/icons';
import _uri from '_utils/_uri';
import _helper from '_utils/_helper';
import _constant from '_utils/_constant';
import { LogDashboardModel } from '_utils/Types';
import * as CSS from 'csstype';
import { useAuth } from '_shared/Contexts/useAuth';
import useNotification from '_shared/Contexts/NotifProvider';

export interface IOrderModel {
  newOnTop: boolean,
  sort: 'name' | 'dtAsc' | 'dtDesc'
}

export default function HeaderBar(props: { 
  query: string,
  showDate: boolean,
  onShowDateChange: (val: boolean) => void,
  useListView: boolean,
  onListViewChange: (val: boolean) => void,
  selectedTiers: string[],
  onTierChange: (tier: string) => void,
  onScanCorrectablePages: (thread: number, res: number) => void,
  order: IOrderModel,
  setOrder: React.Dispatch<React.SetStateAction<IOrderModel>>,
  filterInfo: string,
  loading: boolean
}){
  const { axiosA } = useAuth();
  const [activeKeys, setActiveKeys] = useState<string | string[]>([]);
  const [deleteLogs, setDeleteLogs] = useState<LogDashboardModel[]>([]);
  const [selectedThread, setSelectedThread] = useState<number>(3);
  const [selectedRes, setSelectedRes] = useState<number>(1280);
  const { notif } = useNotification();

  useEffect(() => {
    if(activeKeys.indexOf('deletedAlbums') !== -1){
      axiosA.get<LogDashboardModel[]>(_uri.GetDeleteLogs(props.query))
        .then(res => {
          setDeleteLogs(res.data.sort((a, b) => a.albumFullTitle.localeCompare(b.albumFullTitle)));
        })
        .catch(error => {
          notif.apiError(error);
        });
    }
  }, [props.query, activeKeys]);

  function handleOrderChange(name: string, val: any){
    props.setOrder(prev => {
      //@ts-ignore
      prev[name] = val;
      return {...prev}
    })
  }

  return (
    <Spin spinning={props.loading}>
      <Collapse 
        bordered={false}
        expandIcon={({ isActive }) => <CaretRightFilled rotate={isActive ? 90 : 0} />}
        style={{marginBottom:'10px'}}
        activeKey={activeKeys}
        onChange={setActiveKeys}
      >
        <Collapse.Panel header={
          <div style={{display:'flex'}}>
            <div style={{flex:'1'}}>Filters</div>
            <div style={{flex:'1', textAlign:'right'}}>{props.filterInfo}</div>
          </div>
        } key='filters'>
          <Row gutter={4}>
            <Col sm={8}>
              <Row>
                <Col span={12}>
                  <div>Show Date</div>
                  <Switch checked={props.showDate} onChange={props.onShowDateChange} />
                </Col>
                <Col span={12}>
                  <div>List View</div>
                  <Switch checked={props.useListView} onChange={props.onListViewChange}/>
                </Col>
              </Row>
            </Col>
            <Col sm={8}>
              <Checkbox checked={props.order.newOnTop} onChange={(e) => handleOrderChange('newOnTop', e.target.checked)}>New on Top</Checkbox>
              <Radio.Group value={props.order.sort} onChange={(e) => handleOrderChange('sort', e.target.value)} >
                <Radio value={'name'}>Name</Radio>
                <Radio value={'dtAsc'}>Dt Asc</Radio>
                <Radio value={'dtDesc'}>Dt Desc</Radio>
              </Radio.Group>
            </Col>
            <Col sm={8}>
              <TierChecklist onTierChange={props.onTierChange} selectedTiers={props.selectedTiers} />
            </Col>
          </Row>
          <Row gutter={4}>
            <Col sm={8}>
              <Button onClick={() => props.onScanCorrectablePages(selectedThread, selectedRes)} 
                type='primary' ghost={true} style={{width:'100%'}}
              >
                Scan Correctable Pages
              </Button>
            </Col>
            <Col sm={3}>
              <Select 
                style={{width:'100%'}}
                options={_constant.threadOptions}
                value={selectedThread} onChange={val => setSelectedThread(val)}
              />
            </Col>
            <Col sm={3}>
              <Select 
                style={{width:'100%'}}
                options={_constant.resOptions}
                value={selectedRes} onChange={val => setSelectedRes(val)}
              />
            </Col>
          </Row>
        </Collapse.Panel>
        <Collapse.Panel header={
          <div style={{display:'flex'}}>
            <div style={{flex:'1'}}>Deleted Albums</div>
          </div>
        } key='deletedAlbums'>
          {deleteLogs.map(a => (
            <div key={a.id} style={{display:'flex'}}>
              <div style={{flex:'1'}}>{a.albumFullTitle}</div>
              <div>
                {_helper.formatNullableDatetime(a.creationTime)}
              </div>
            </div>
          ))}
        </Collapse.Panel>
      </Collapse>
    </Spin>
  );
}

function TierChecklist(props:{
  selectedTiers: string[],
  onTierChange: (tier: string) => void
}){
  const getButtonDisplay = (tier: string):{
    type: "primary" | "link",
    ghost: boolean,
    style: CSS.Properties
  } => {
    const isTierMatch = props.selectedTiers.includes(tier);

    return {
      type: isTierMatch ? "primary" : "link",
      ghost: isTierMatch,
      style: {
        width: "100%"
      }
    }
  };

  return(
    <Row>
      <Col span={4}></Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('0')} {...getButtonDisplay('0')}>0</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('1')} {...getButtonDisplay('1')}>1</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('2')} {...getButtonDisplay('2')}>2</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('3')} {...getButtonDisplay('3')}>3</Button>
      </Col>
      <Col span={4} className="headerbutton-container">
        <Button onClick={() => props.onTierChange('N')} {...getButtonDisplay('N')}>N</Button>
      </Col>
    </Row>
  );
}