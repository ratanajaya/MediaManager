import { Row, Col, Typography } from 'antd';
import useSWR from 'swr';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import _uri from "_utils/_uri";
import { AlbumCardGroup, AlbumCardModel } from '_utils/Types';
import _helper from '_utils/_helper';
import { useNavigate } from 'react-router-dom';
import _constant from '_utils/_constant';

export default function Genres(props: {
  page: string,
}) {
  const uri = _helper.equalIgnoreCase(props.page, "Genres") ? _uri.GetGenreCardModels() 
    : _helper.equalIgnoreCase(props.page, "Artists") ? _uri.GetFeaturedArtistCardModels() 
    : _helper.equalIgnoreCase(props.page, "Characters") ? _uri.GetFeaturedCharacterCardModels() 
    : null;

  const navigate = useNavigate();

  const { data: albumCGroups, error } = useSWR<AlbumCardGroup[], any>(uri);

  if (error) { return <Typography.Text>Error!</Typography.Text>; }
  if (!albumCGroups) { return <Typography.Text>Loading {props.page.toLowerCase()}...</Typography.Text>; }

  const readerHandler = {
    view: function (albumCm: AlbumCardModel) {
      navigate('/Albums?query=' + albumCm.path)
    },
  }

  return (
    <>
      {albumCGroups.map((acg, index) =>{
        return (
          <Row key={acg.name} gutter={0}>
            {acg.albumCms.map((a) => (
              <Col key={"albumCol" + a.path} style={{..._constant.colStyle}} {..._constant.colProps}>
                <MyAlbumCard
                  albumCm={a}
                  onView={readerHandler.view}
                  onEdit={() => { }}
                  showContextMenu={false}
                  showPageCount={true}
                  getCoverSrc={(coverInfo) => _uri.StreamResizedImage(coverInfo.libRelPath, 150, 0)}
                />
              </Col>
            ))}
          </Row>
        );
      })}
    </>
  );
}