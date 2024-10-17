import { useState, useEffect, useMemo, CSSProperties } from 'react';

import { Row, Col, Grid } from 'antd';
import { ExclamationCircleOutlined } from '@ant-design/icons';

import { InView } from 'react-intersection-observer';

import MyAlbumCard from '_shared/Displays/MyAlbumCard';
import ReaderModal from '_shared/ReaderModal';
import EditModal from 'pages/Albums/EditModal';
import _uri from '_utils/_uri';
import _helper from '_utils/_helper';
import _constant from '_utils/_constant';
import MyAlbumWideCard from '_shared/Displays/MyAlbumWideCard';
import { AlbumCardModel, AlbumVM, QueryPart, PathCorrectionModel } from '_utils/Types';

import QmarkCyan from '_assets/resources/qmark-cyan.png';
import QmarkGreen from '_assets/resources/qmark-green.png';
import QmarkYellow from '_assets/resources/qmark-yellow.png';

import cssVariables from '_assets/styles/cssVariables';
import HeaderBar, { IOrderModel } from 'pages/Albums/HeaderBar';
import _ls from '_utils/_ls';
import { useAuth } from '_shared/Contexts/useAuth';
import { AlbumDownloadModal } from '_shared/DownloadModal';
import useNotification from '_shared/Contexts/NotifProvider';
import { useModal } from '_shared/Contexts/ModalProvider';
import { useNavigate } from 'react-router-dom';

const { useBreakpoint } = Grid;

export default function Albums(props: {
  queryParts: QueryPart,
}) {
  const type = 0;

  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);

  //#region Display Album List
  const pageSize = 100;
  const [albumCms, setAlbumCms] = useState<AlbumCardModel[]>([]);
  const [maxPage, setMaxPage] = useState(0);
  const screens = useBreakpoint();
  const itemPerRow = screens.xs ? 2 : screens.sm && !screens.md ? 4 : 8;
  const [loading, setLoading] = useState(false);
  const [useListView, setUseListView] = useState(false);
  const [showDate, setShowDate] = useState(false);
  const [selectedTiers, setSelectedTiers] = useState<string[]>([]);
  const [order, setOrder] = useState<IOrderModel>({
    newOnTop: true,
    sort:'name'
  });
  const navigate = useNavigate();
  const { notif } = useNotification();

  const { axiosA } = useAuth();
  const { modal } = useModal();

  useEffect(() =>{
    const storedSelectedTiers = _ls.load<string[]>(_constant.lsKey.selectedTiers);
    setSelectedTiers(_helper.nzAny<string[]>(storedSelectedTiers, ['N','0','1','2','3']));
  },[]);

  const handleTierChange = (tier: string) => {
    const newTier = selectedTiers.includes(tier)
      ? selectedTiers.filter(e => e !== tier)
      : [...selectedTiers, tier];

    setSelectedTiers(newTier);

    _ls.set(_constant.lsKey.selectedTiers, newTier);
  }

  const handleRefresh = () => {
    return new Promise((resolve, reject) => {
      axiosA.get<AlbumCardModel[]>(_uri.GetAlbumCardModels(type, 0, 0, props.queryParts.query))
        .then((response) => {
          setAlbumCms(response.data);
          resolve("param from resolve");
        })
        .catch((error) => {
          notif.apiError(error);
          reject("param from reject");
        })
        .finally(() => { })
    });
  }

  useEffect(() => {
    handleRefresh();
  }, [props.queryParts.query]);
  
  function inViewChange(inView: any, entry: any){
    if(entry.isIntersecting) setMaxPage(maxPage + 1);
  }
  //#endregion

  //#region Display Album Pages
  const [selectedAlbumCm, setSelectedAlbumCm] = useState<AlbumCardModel | null>(null);
  useEffect(() => {
    document.body.style.overflow = selectedAlbumCm != null ? 'hidden' : 'visible';
  },[selectedAlbumCm]);

  const readerHandler = {
    view: (albumCm: AlbumCardModel) => {
      setSelectedAlbumCm(albumCm);
    },
    randomView: (selector: (acm: AlbumCardModel) => boolean ) => {
      const possibleAlbums = albumCms.filter(selector);
      const acm = possibleAlbums[_helper.getRandomInt(0, possibleAlbums.length)];
      readerHandler.view(acm);
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

      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          lastPageIndex: lastPageIndex,
          isRead: lastPageIndex === albumCm.pageCount - 1 ? true : albumCm.isRead
        };
      }));
    }
  }
  //#endregion

  //#region Edit Delete Download
  const [editedAlbumPath, setEditedAlbumPath] = useState<string | null>(null);
  const [downloadedAlbumCm, setDownloadedAlbumCm] = useState<AlbumCardModel | null>(null);

  const crudHandler = {
    edit: (path: string) => {
      setEditedAlbumPath(path);
    },
    pageDeleteSuccess: (path: string) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: albumCm.pageCount - 1
        };
      }));
    },
    chapterDeleteSuccess: (path: string, newPageCount: number) => {
      setAlbumCms(albumCms.map((albumCm, aIndex) => {
        if (albumCm.path !== path) {
          return albumCm;
        }
        return {
          ...albumCm,
          pageCount: newPageCount,
          lastPageIndex: 0
        };
      }));
    }
  }

  const editHandler = {
    ok: (editedAlbumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      axiosA.post(_uri.UpdateAlbum(type), editedAlbumVm)
        .then((response) => {
          const newAlbumCms = [...albumCms];
          const editedAlbumVmIndex = newAlbumCms.findIndex(a => a.path === editedAlbumVm.path);
          newAlbumCms[editedAlbumVmIndex].title = editedAlbumVm.album.title;
          newAlbumCms[editedAlbumVmIndex].artistDisplay = `${editedAlbumVm.album.artists.join(', ')}`;
          newAlbumCms[editedAlbumVmIndex].languages = editedAlbumVm.album.languages;
          newAlbumCms[editedAlbumVmIndex].note = editedAlbumVm.album.note;
          newAlbumCms[editedAlbumVmIndex].tier = editedAlbumVm.album.tier;
          newAlbumCms[editedAlbumVmIndex].isWip = editedAlbumVm.album.isWip;

          setAlbumCms(newAlbumCms);
        })
        .catch((error) => {
          notif.apiError(error);
        });
    },
    cancel: () => {
      setEditedAlbumPath(null);
    },
    delete: (albumVm: AlbumVM) => {
      const album = albumVm.album;
      modal.confirm({
        title: `Delete album [${album.artists.join(', ')}] ${album.title}?`,
        icon: <ExclamationCircleOutlined />,
        okText: '   YES   ',
        okType: 'danger',
        cancelText: '   NO   ',
        onOk() {
          editHandler.directDelete(albumVm.path);
        },
        onCancel() {
        },
      });
    },
    directDelete: (path: string) => {
      axiosA.delete(_uri.DeleteAlbum(type, path))
        .then(function (response) {
          setAlbumCms(albumCms.filter(albumCm => {
            return albumCm.path !== path;
          }));
        })
        .catch(function (error) {
          notif.apiError(error);
        })
        .finally(() => {
          setEditedAlbumPath(null);
        })
    },
    refresh: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      axiosA.get<string>(_uri.RefreshAlbum(0, albumVm.path))
        .then((response) => {
          notif.info("Refresh Album", response.data, null);
        })
        .catch((error) => {
          notif.apiError(error);
        });
    },
    editFiles: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      navigate("/FileManagement?path=" + albumVm.path);
    },
    correctFiles: (albumVm: AlbumVM) => {
      setEditedAlbumPath(null);
      navigate("/ScCorrection?path=" + encodeURIComponent(albumVm.path));
    },
    download: (albumVm: AlbumVM) => {
      setDownloadedAlbumCm(albumCms.find(a => a.path === albumVm.path)!);
      setEditedAlbumPath(null);
    },
  }
  //#endregion

  function handleScanCorrectablePages(thread: number, res: number){
    setLoading(true);

    axiosA.post<PathCorrectionModel[]>(_uri.HScanCorrectiblePaths(), { paths: facmPaths, thread:thread, upscaleTarget:res })
      .then((response) => {
        setAlbumCms(prev => {
          const newAcms = prev.map(acm => {
            const pcm = response.data.find(a => a.libRelPath === acm.path);
            return {
              ...acm,
              correctablePageCount: pcm?.correctablePageCount ?? acm.correctablePageCount,
            }
          });

          return [...newAcms];
        });
      })
      .catch((error) => {
        notif.apiError(error);
      })
      .finally(() => {
        setLoading(false);
      });
  }
  
  const randomAlbums = [
    {
      name: "RANDOM_NEW",
      img: QmarkYellow,
      selector: (acm: AlbumCardModel) => { return !acm.isRead }
    },
    {
      name: "RANDOM_T2",
      img: QmarkGreen,
      selector: (acm: AlbumCardModel) => { return acm.tier === 2 }
    },
    {
      name: "RANDOM_T3",
      img: QmarkCyan,
      selector: (acm: AlbumCardModel) => { return acm.tier === 3 }
    }
  ]

  const [memoizedAlbumList, facmLength, facmPaths] = useMemo(() => {
    function albumCmSorter(a: AlbumCardModel, b: AlbumCardModel){
      const isReadComparison = !order.newOnTop ? 0 : (a.isRead ? 1 : 0) - (b.isRead ? 1 : 0);
      
      if(isReadComparison !== 0)
        return isReadComparison;

      if(order.sort === 'name'){
        const artistComparison = a.artistDisplay.localeCompare(b.artistDisplay);
        return artistComparison !== 0 ? artistComparison : a.title.localeCompare(b.title);
      }
      else if(order.sort === 'dtAsc'){
        return a.entryDate.localeCompare(b.entryDate);
      }
      else if(order.sort === 'dtDesc'){
        return b.entryDate.localeCompare(a.entryDate);
      }
  
      return 1;
    }

    const filteredAlbumCms = albumCms.filter(a => { 
      return !a.isRead && a.tier === 0 
        ? selectedTiers.includes('N') 
        : selectedTiers.includes(`${a.tier}`);
    });

    const addRandomAlbum = filteredAlbumCms.length >= 100;
    const offset = addRandomAlbum ? 3 : 0;
  
    const pagedAlbumCms = (() => {
      const orderedAlbumCms = filteredAlbumCms.sort(albumCmSorter);
  
      const itemCount = !useListView ? maxPage * pageSize : filteredAlbumCms.length;
      return orderedAlbumCms.slice(0, itemCount);
    })();
    
    return[
      (
        <>
          {!useListView ? 
            <>
              <Row gutter={0}>
                {addRandomAlbum && 
                  randomAlbums.map((a, index) => 
                    {
                      const rowIndex = Math.floor(index / itemPerRow);
                      const bc = (index + rowIndex) % 2 === 0 ? cssVariables.highlightLight : 'inherit';
                      return (
                        <Col 
                          style={{
                            ..._constant.colStyle,
                            backgroundColor: bc 
                          }} 
                          {..._constant.colProps} 
                          key={a.name}
                        >
                          <div className='my-album-card-container-1'>
                            <div className='my-album-card-container-2'>
                              <div className='my-album-card-container-3' onClick={() => readerHandler.randomView(a.selector)}>
                                <img className='my-album-card-img' alt="img" src={a.img}/>
                              </div>
                            </div>
                            <span className='my-album-card-title'>
                              {a.name}
                            </span>
                          </div>
                        </Col>
                      );
                    }
                  )
                }
                {pagedAlbumCms.map((a, index) => {
                  const ofsetedIndex = index + offset;
                  const rowIndex = Math.floor(ofsetedIndex / itemPerRow);
                  const bc = (ofsetedIndex + rowIndex) % 2 === 0 ? cssVariables.highlightLight : 'inherit';
                  return (
                    <Col 
                      key={a.path}
                      style={{
                        ..._constant.colStyle,
                        backgroundColor:bc,
                      }} 
                      {..._constant.colProps} 
                    >
                      <MyAlbumCard
                        albumCm={a}
                        onView={readerHandler.view}
                        onEdit={crudHandler.edit}
                        showContextMenu={true}
                        showDate={showDate}
                        showPageCount={true}
                        getCoverSrc={(coverInfo) => _uri.StreamResizedImage(coverInfo.libRelPath, 150, 0)}
                      />
                    </Col>
                  );
                })}
              </Row>
              <InView as="div" onChange={inViewChange}>
                <div style={{width:"100%", height:"5px", backgroundColor:"transparent"}}></div>
              </InView>
            </> :
            <div style={{paddingRight:"10px"}}>
              {pagedAlbumCms.map((a, index) => {
                return(
                  <MyAlbumWideCard albumCm={a}
                    onView={readerHandler.view}
                    onEdit={crudHandler.edit}
                    onDelete={editHandler.directDelete}
                    type={0}
                  />
                );
              })}
            </div>
          }
        </>
      ),
      filteredAlbumCms.length,
      filteredAlbumCms.map(a => a.path)
    ]
  },[ albumCms, itemPerRow, maxPage, useListView, showDate, order, selectedTiers ]);

  return (
    <>
      {downloadedAlbumCm != null &&
        <AlbumDownloadModal
          albumCm={downloadedAlbumCm}
          onClose={() => { setDownloadedAlbumCm(null); }}
        />
      }
      <HeaderBar 
        query={props.queryParts.query}
        showDate={showDate} onShowDateChange={setShowDate}
        useListView={useListView} onListViewChange={setUseListView}
        selectedTiers={selectedTiers} onTierChange={handleTierChange}
        onScanCorrectablePages={handleScanCorrectablePages}
        filterInfo={`Total: ${albumCms.length} Filtered: ${facmLength}`}
        order={order}
        setOrder={setOrder}
        loading={loading}
      />
      {memoizedAlbumList}
      <ReaderModal
        onClose={readerHandler.close}
        onChapterDeleteSuccess={crudHandler.chapterDeleteSuccess}
        onOpenEditModal={()=>{
          setEditedAlbumPath(selectedAlbumCm?.path ?? null)
        }}
        albumCm={selectedAlbumCm}
        type={0}
      />
      {editedAlbumPath != null &&
        <EditModal
          albumPath={editedAlbumPath}
          onOk={editHandler.ok}
          onCancel={editHandler.cancel}
          onDelete={editHandler.delete}
          onRefresh={editHandler.refresh}
          onDownload={editHandler.download}
          onEditFiles={editHandler.editFiles}
          onCorrectFiles={editHandler.correctFiles}
          type={0}
        />
      }
    </>
  );
}