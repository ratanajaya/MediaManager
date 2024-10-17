import { useState, useEffect } from 'react';

import { Row, Col, Typography } from 'antd';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import _uri from '_utils/_uri';
import _constant from '_utils/_constant';
import { AlbumCardModel, QueryPart } from '_utils/Types';
import useSWR from 'swr';
import _helper from '_utils/_helper';
import ReaderModal from '_shared/ReaderModal';
import { useAuth } from '_shared/Contexts/useAuth';
import useNotification from '_shared/Contexts/NotifProvider';
import { useNavigate } from 'react-router-dom';

const type = 1;

export default function SelfComps(props: {
  queryParts: QueryPart,
}) {
  const { axiosA } = useAuth();
  const { notif } = useNotification();
  const navigate = useNavigate();
  
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  const [selectedAlbumCm, setSelectedAlbumCm] = useState<AlbumCardModel | null>(null);

  const uri = _uri.GetAlbumCardModels(type, 0, 0, props.queryParts.path ?? "")
  const { data: albumCms, error, mutate } = useSWR<AlbumCardModel[], any>(uri);

  if (error) { return <pre>{JSON.stringify(error, undefined, 2)}</pre>; }
  if (!albumCms) { return <Typography.Text>Loading {_helper.nz(props.queryParts.path, "SelfComp")}...</Typography.Text>; }

  const readerHandler = {
    view: (pageCount: number, albumCm: AlbumCardModel) => {
      if(pageCount === 0){
        navigate("/Sc?path=" + albumCm.path);
      }
      else{
        setSelectedAlbumCm(albumCm);
      }
    },

    close: (path: string, lastPageIndex: number) => {
      setSelectedAlbumCm(null);

      axiosA.post(_uri.UpdateAlbumOuterValue(type), {
        albumPath: path,
        lastPageIndex: lastPageIndex
      })
        .then((response) => { })
        .catch((error) => {
          notif.apiError(error);
        });
        
        mutate(() => albumCms.map((albumCm, aIndex) => {
            if (albumCm.path !== path) {
              return albumCm;
            }
            return {
              ...albumCm,
              lastPageIndex: lastPageIndex
            };
          }), false);
    }
  }

  const crudHandler = {
    chapterDeleteSuccess: (path: string, newPageCount: number) => {
      mutate(() => albumCms.map((albumCm, aIndex) => {
          if (albumCm.path !== path) {
            return albumCm;
          }
          return {
            ...albumCm,
            pageCount: newPageCount,
            lastPageIndex: 0
          };
        }), false);
    }
  }

  return (
    <>
      {/* <PullToRefresh onRefresh={mutate}> */}
      <Row gutter={0}>
        {albumCms.map((a, index) => (
          <Col style={{..._constant.colStyle}} {..._constant.colProps} key={a.path}>
            <MyAlbumCard
              albumCm={a}
              onView={(albumCm) => readerHandler.view(a.pageCount, albumCm)}
              showContextMenu={true}
              showPageCount={a.pageCount > 0}
              getCoverSrc={(coverInfo) => _uri.StreamResizedImage(coverInfo.libRelPath, 150, 1)}
            />
          </Col>
        ))}
      </Row>
      {/* </PullToRefresh> */}
      {selectedAlbumCm !== null && 
        <ReaderModal
          onClose={readerHandler.close}
          onChapterDeleteSuccess={crudHandler.chapterDeleteSuccess}
          onOpenEditModal={() => {}}
          albumCm={selectedAlbumCm}
          type={type}
        />
      }
      
    </>
  );
}