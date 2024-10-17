import { useState, useEffect } from 'react';

import { Row, Col } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';
import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import _helper from '_utils/_helper';
import _constant from '_utils/_constant';
import { AlbumCardModel, OfflineAlbumCm, OfflineFile } from '_utils/Types';

import _db from '_utils/_db';
import OfflineReaderModal from './OfflineReaderModal';
import useNotification from '_shared/Contexts/NotifProvider';
import { useModal } from '_shared/Contexts/ModalProvider';

export default function OfflineAlbums() {
  const [offlineAlbums, setOfflineAlbums] = useState<OfflineAlbumCm[]>([]);
  const [coverFiles, setCoverFiles] = useState<OfflineFile[]>([]);
  const { notif } = useNotification();
  const { modal } = useModal();

  useEffect(() => {
    window.scrollTo(0, 0);
    _db.albumCms.toArray().then((resAcms) => {
      setOfflineAlbums(resAcms);
      const coverAlRelPaths = resAcms
        .filter(a => !_helper.isVideo(a.coverInfo.extension))
        .map(a => a.coverInfo.libRelPath.replace(`${a.path}\\`, ''));

      _db.offlineFiles.filter(e => coverAlRelPaths.includes(e.alRelPath)).toArray().then((resOfflineFiles) => {
        setCoverFiles(resOfflineFiles);
      });
    });
  }, []);

  const [selectedOfflineAlbum, setSelectedOfflineAlbum] = useState<OfflineAlbumCm | null>(null);
  useEffect(() => {
    document.body.style.overflow = selectedOfflineAlbum != null ? 'hidden' : 'visible';
  },[selectedOfflineAlbum]);

  const readerHandler = {
    view: (albumCm: AlbumCardModel) => {
      const offlineAlbum = offlineAlbums.find(e => e.path === albumCm.path);
      setSelectedOfflineAlbum(offlineAlbum!);
    },
    close: (path: string, lastPageIndex: number) => {
      _db.albumCms.update(path, { lastPageIndex: lastPageIndex })
        .then(() => {
          setSelectedOfflineAlbum(null);
        })
        .catch((err) => {});
    }
  }

  const editHandler = {
    directDelete: (path: string) => {
      
    },
    delete: (path: string) => {
      modal.confirm({
        title: `Delete album ${path}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   YES   ',
        okType: 'danger',
        cancelText: '   NO   ',
        onOk() {
          _db.albumCms.delete(path).then(() => {
            setOfflineAlbums(prev => {
              return prev.filter(e => e.path !== path);
            });

            _db.offlineFiles.where('albumPath').equals(path).delete().then(() => {
              }).catch((err) => {
                notif.apiError(err);
              });

          }).catch((err) => {
            notif.apiError(err);
          });
        },
        onCancel() {
        },
      });
    },
  }

  const sortedOfflineAlbums = offlineAlbums.sort((a, b) => {
    const artistComparison = a.artistDisplay?.localeCompare(b.artistDisplay);
    return artistComparison !== 0 ? artistComparison : a.title.localeCompare(b.title);
  });

  return (
    <>
      <Row gutter={0}>
        {sortedOfflineAlbums.map((a, i) => (
          <Col 
            key={a.path} 
            style={{ textAlign: 'center' }}
            {..._constant.colProps}
          >
            <MyAlbumCard
              albumCm={a}
              showContextMenu={true}
              onView={() => readerHandler.view(a)}
              onEdit={editHandler.delete}
              getCoverSrc={(coverInfo) => {
                if(coverFiles.length === 0 || _helper.isVideo(coverInfo.extension))
                  return _constant.imgPlaceholder;

                const coverAlRelPath = coverInfo.libRelPath.replace(`${a.path}\\`, '');
                const coverFile = coverFiles.find(e => e.albumPath === a.path && e.alRelPath === coverAlRelPath);

                if(coverFile){
                  return URL.createObjectURL(coverFile.blob);
                }

                return _constant.imgPlaceholder;
              }}
            />
          </Col>
        ))}
      </Row>
      {selectedOfflineAlbum != null &&
        <OfflineReaderModal 
          albumCm={selectedOfflineAlbum}
          onClose={readerHandler.close}
        />
      }
    </>
  )
}