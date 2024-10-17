import React, { useState, useEffect } from 'react';
import { Environment, LogDashboardModel, TablePaginationModel } from '_utils/dashboardTypes';
import Table from 'react-bootstrap/Table';
import moment from 'moment';
import Badge from 'react-bootstrap/Badge';
import { Form, Stack, Row, Col, Button, Pagination } from 'react-bootstrap';
import { useAuth } from '_shared/Contexts/useAuth';
import _uri from '_utils/_uri';
import _helper from '_utils/_helper';
import useNotification from '_shared/Contexts/NotifProvider';

export default function Logs() {
  const [tableModel, setTableModel] = useState<TablePaginationModel<LogDashboardModel>>({
    totalItem: 0,
    totalPage: 0,
    records:[]
  });

  const defaultFilter = {
    page: 1,
    row: 15,
    freetext: '',
    operation: '',
    startDate: '',
    endDate: '',
    shouldRefresh: false
  };
  const [filter, setFilter] = useState<{
    page: number,
    row: number,
    freetext: string,
    operation: string,
    startDate: string,
    endDate: string,
    shouldRefresh: boolean
  }>(defaultFilter);
  
  const { axiosA } = useAuth();
  const { notif } = useNotification();

  function refreshTable(){
    const url = _uri.GetLogs(filter.page, filter.row, filter.operation, filter.freetext, filter.startDate, filter.endDate);

    axiosA.get<TablePaginationModel<LogDashboardModel>>(url)
      .then((response) => {
        setTableModel(response.data);
      })
      .catch((error) => {
        notif.apiError(error);
      })
      .finally(() => {
        setFilter(prev => {
          prev.shouldRefresh = false;
          return {...prev};
        });
      });
  }

  useEffect(() => {
    refreshTable();
  },[]);

  useEffect(() => {
    if(!filter.shouldRefresh) return;

    refreshTable();
  },[filter])

  function movePage(increment: number){
    const newPage = filter.page + increment;
    if(newPage < 1 || newPage > tableModel.totalPage)
      return;

    setFilter(prev => {
      return {
        ...prev,
        shouldRefresh: true,
        page: newPage
      };
    });
  }

  return (
    <div className=' pl-2 pr-2'>
      <div>
        <Form>
          <Stack gap={1} style={{marginBottom:'10px'}}>
            <Row>
              <Col sm={2} xs={4} className="label-col">
                Search :
              </Col>
              <Col sm={10} xs={8}>
                <Form.Control 
                  placeholder="Search" size="sm"
                  value={filter.freetext}  
                  onChange={(event) => { 
                    setFilter(prev => {
                      prev.freetext = event.target.value;
                      return {...prev};
                    }) 
                  }}
                  />
              </Col>
            </Row>
            <Row>
              <Col sm={2} xs={4} className="label-col">
                Operation :
              </Col>
              <Col sm={10} xs={8}>
                <Form.Select 
                  size="sm"
                  value={filter.operation} 
                  onChange={(event) => { 
                    setFilter(prev => {
                      prev.operation = event.target.value;
                      return {...prev};
                    })
                  }}
                >
                  <option value="">-</option>
                  <option value="I">Insert</option>
                  <option value="U">Update</option>
                  <option value="D">Delete</option>
                  <option value="F">FirstRead</option>
                </Form.Select>
              </Col>
            </Row>
            <Row>
              <Col sm={2} xs={4} className="label-col">
                Date :
              </Col>
              <Col sm={10} xs={8}>
                <div style={{display:'flex'}}>
                  <Form.Control 
                    type="date" size="sm" style={{flex:'12'}}
                    value={filter.startDate}
                    onChange={(event) => { 
                      setFilter(prev => {
                        prev.startDate = event.target.value;
                        return {...prev};
                      })
                    }}
                  />
                  <span style={{flex:'1'}}>-</span>
                  <Form.Control 
                    type="date" size="sm" style={{flex:'12'}} 
                    value={filter.endDate}
                    onChange={(event) => {
                      setFilter(prev => {
                        prev.endDate = event.target.value;
                        return {...prev};
                      })
                    }}
                  />
                </div>
              </Col>
            </Row>
            <Stack direction='horizontal' gap={1} style={{justifyContent:'flex-end'}}>
              <div style={{flex:'1', textAlign:'center'}}>
                <span>{filter.page} / {tableModel.totalPage} [{tableModel.totalItem}]</span>
              </div>
              <Button 
                variant="secondary" size="sm" style={{width:'100px'}}
                onClick={() => {
                  setFilter({
                    ...defaultFilter, 
                    shouldRefresh:true
                  });
                }}
              >
                Reset
              </Button>
              <Button 
                variant="success" size="sm" style={{width:'100px'}}
                onClick={refreshTable}
              >
                Search
              </Button>
            </Stack>
          </Stack>
        </Form>
      </div>
      <div style={{position:'relative'}}>
        <Table striped bordered hover variant="dark" style={{tableLayout:'auto', borderCollapse:'collapse', width:'100%' }}>
          <thead>
            <tr>
              <th>#</th>
              <th style={{minWidth:'150px'}}>Date</th>
              <th>Op</th>
              <th style={{width:'100%'}}>Album Title</th>
            </tr>
          </thead>
          <tbody>
            {tableModel.records.map((log, index) => {
              const badgeType = log.operation === 'D' ? 'danger' : log.operation === 'U' ? 'primary' : log.operation === 'F' ? 'success' : 'warning';
              return (
                <tr key={index}>
                  <td>{1 + (index + (filter.row * (filter.page - 1)))}</td>
                  <td>{moment(log.creationTime).format('yyyy-MM-DD HH:mm')}</td>
                  <td><Badge bg={badgeType} style={{width:'24px'}}>{log.operation}</Badge></td>
                  <td style={{textAlign:'left'}}>{log.albumFullTitle}</td>
                </tr>
              ); 
            })}
          </tbody>
        </Table>
        <OverlayButton 
          location='left'
          onClick={() => { movePage(-1); }}
        />
        <OverlayButton 
          location='right'
          onClick={() => { movePage(1); }}        
        />
      </div>
    </div>
  )
}

function OverlayButton(props:{
  location: 'right' | 'left',
  onClick: () => void
}){
  const iconClass = props.location === 'right' ? 'arrow-right' : 'arrow-left';
  const style = props.location === 'right' ? { right:'0px' } : { left:'0px' };
  const [showIcon, setShowIcon] = useState(false);

  return(
    <div
      className='overlay-button center-container'
      style={style}
      onClick={props.onClick}
      onMouseEnter={() => { setShowIcon(true) }}
      onMouseLeave={() => { setShowIcon(false) }}
    >
      {showIcon ? <div className={iconClass}></div> : <></>}      
    </div>
  );
}