import { useState, useEffect } from 'react';
import { useNavigate } from "react-router-dom";

import { Button, Row, Col, Modal, Typography } from 'antd';
import { SearchOutlined } from '@ant-design/icons';
import TextArea from 'antd/lib/input/TextArea';
import _helper from "_utils/_helper";
import _constant from "_utils/_constant";
import { useAlbumInfo } from '_shared/Contexts/AlbumInfoProvider';

export default function QueryEditor(props:{
  query: string,
}) {
  const navigate = useNavigate();
  const [visible, setVisible] = useState(false);
  const [query, setQuery] = useState("");

  const albumInfo = useAlbumInfo();

  useEffect(() => {
    setQuery(props.query);
  }, [props.query]);

  const queryHandler = {
    change: function(clause: string, val: string) {
      const targetQueryPlus = `${clause}:${val}`;
      const targetQueryMinus = `${clause}!${val}`;

      const currentQueries = query !== "" ? query.split(',') : [];
      const cleanedQueries = currentQueries.filter((item, index) => !(item === targetQueryPlus || item === targetQueryMinus));

      const itemToAdd = !currentQueries.includes(targetQueryPlus) && !currentQueries.includes(targetQueryMinus) ? [targetQueryPlus] 
        : !currentQueries.includes(targetQueryMinus) ? [targetQueryMinus] : [];

      const newQueries = [...cleanedQueries, ...itemToAdd];

      setQuery(newQueries.join(','));
    },
    goToAlbumList: function () {
      const currentUrlParams = new URLSearchParams(window.location.search);
      currentUrlParams.set('query', query);
      setVisible(false);
      navigate("/Albums?" + currentUrlParams.toString());
    }
  }
  
  function handleKeyDown(e: any){
    if(e.keyCode === 13 && e.shiftKey === false) {
      e.preventDefault();
      queryHandler.goToAlbumList();
    }
  }

  function getSelectStatus(clause: string, tag: string) {
    const queryArr = query.split(',');

    return queryArr.some((str) => str.toLowerCase() === `${clause}:${tag.toLowerCase()}`) ? 'plus' :
      queryArr.some((str) => str.toLowerCase() === `${clause}!${tag.toLowerCase()}`) ? 'minus' :
        null;
  }

  return (
    <>
      <div onClick={() => setVisible(true)} className="h-full">
        Query Editor
      </div>
      <Modal
        open={visible}
        onCancel={() => setVisible(false)}
        closable={false}
        footer={
          <Button type="primary" onClick={queryHandler.goToAlbumList}>
            <SearchOutlined />Search
          </Button>
        }
      >
        <TextArea
          onChange={(event) => setQuery(event.target.value)}
          value={query}
          onKeyDown={handleKeyDown}
        />
        { !albumInfo ? <Typography.Text>loading tags...</Typography.Text> : 
          <>
            <Row style={{marginTop:'10px'}}>
              {albumInfo.povs.map((val, index) => {
                const clause = 'pov';
                const selectStatus = getSelectStatus(clause,val);

                return (
                  <Col span={6} key={val}>
                    <Button
                      ghost={selectStatus !== null}
                      danger={selectStatus === 'minus'}
                      type={selectStatus ? "primary" : "link"}
                      onClick={() => { queryHandler.change(clause,val); }}
                      style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                    >
                      {val}
                    </Button>
                  </Col>
                );
              })
              }
            </Row>
            <Row style={{marginTop:'10px'}}>
              {albumInfo.focuses.map((val, index) => {
                const clause = 'focus';
                const selectStatus = getSelectStatus(clause,val);

                return (
                  <Col span={6} key={val}>
                    <Button
                      ghost={selectStatus !== null}
                      danger={selectStatus === 'minus'}
                      type={selectStatus ? "primary" : "link"}
                      onClick={() => { queryHandler.change(clause,val); }}
                      style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                    >
                      {val}
                    </Button>
                  </Col>
                );
              })
              }
            </Row>
            <Row style={{marginTop:'10px'}}>
              {albumInfo.others.map((val, index) => {
                const clause = 'other';
                const selectStatus = getSelectStatus(clause,val);

                return (
                  <Col span={6} key={val}>
                    <Button
                      ghost={selectStatus !== null}
                      danger={selectStatus === 'minus'}
                      type={selectStatus ? "primary" : "link"}
                      onClick={() => { queryHandler.change(clause,val); }}
                      style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                    >
                      {val}
                    </Button>
                  </Col>
                );
              })
              }
            </Row>
            <Row style={{marginTop:'10px'}}>
              {albumInfo.rares.map((val, index) => {
                const clause = 'rare';
                const selectStatus = getSelectStatus(clause,val);

                return (
                  <Col span={6} key={val}>
                    <Button
                      ghost={selectStatus !== null}
                      danger={selectStatus === 'minus'}
                      type={selectStatus ? "primary" : "link"}
                      onClick={() => { queryHandler.change(clause,val); }}
                      style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                    >
                      {val}
                    </Button>
                  </Col>
                );
              })
              }
            </Row>
            <Row style={{marginTop:'10px'}}>
              {albumInfo.qualities.map((val, index) => {
                const clause = 'quality';
                const selectStatus = getSelectStatus(clause, val);

                return (
                  <Col span={6} key={val}>
                    <Button
                      ghost={selectStatus !== null}
                      danger={selectStatus === 'minus'}
                      type={selectStatus ? "primary" : "link"}
                      onClick={() => { queryHandler.change(clause,val); }}
                      style={{ width: "100%", textAlign: "left", marginBottom: "2px" }}
                    >
                      {val}
                    </Button>
                  </Col>
                );
              })
              }
            </Row>
          </>
        }
      </Modal>
    </>
  );
}