import { useState, useEffect, useMemo } from 'react';

import ReaderModalContextMenu from '_shared/ReaderModal/ReaderModalContextMenu';
import _constant from '_utils/_constant';
import _helper from '_utils/_helper';
import _uri from '_utils/_uri';
import { AlbumCardModel, AlbumFsNodeInfo, FileInfoModel, NodeType } from '_utils/Types';
import { IsScreenPortrait } from '_utils/_display';

import ChapterDrawer from '_shared/ReaderModal/ChapterDrawer';
import Spinner from '_shared/Spinner';
import { useSetting } from '_shared/Contexts/SettingProvider';
import { useAuth } from '_shared/Contexts/useAuth';
import ReaderDisplay, { AlbumPaginationModel, PagingIndex } from '_shared/ReaderModal/ReaderDisplay';
import Overlay3x3Nav from '_shared/ReaderModal/Overlay3x3Nav';
import useNotification from '_shared/Contexts/NotifProvider';
import usePageManager from '_shared/Contexts/usePageManager';
import TranslateDisplay from './TranslateDisplay';
import { useNavigate } from 'react-router-dom';

export default function ReaderModal(props: {
  albumCm: AlbumCardModel | null,
  type: 0 | 1,
  onOpenEditModal: () => void,
  onChapterDeleteSuccess: (path: string, val: number) => void,
  onClose: (path: string, lpi: number) => void
}) {
  const { axiosA } = useAuth();
  const { notif } = useNotification();
  const { pageManager } = usePageManager();

  const [apm, setApm] = useState<AlbumPaginationModel>({
    path: "",
    lpi: 0,
    fsNodes: [],
    indexes: _constant.defaultIndexes,
    orientation: 'Portrait',
    detailLevel: 'none',
  });
  const [ ocrMode, setOcrMode ] = useState<boolean>(false);
  const { setting } = useSetting();
  
  const navigate = useNavigate();

  const [loading, setLoading] = useState<boolean>(false);
  
  const pages = useMemo(() => {
    return _helper.getFlatFileInfosFromFsNodes(apm.fsNodes);
  }, [apm.fsNodes]);

  useEffect(() => {
    if (props.albumCm == null){
      setApm(prev => {
        prev.path = "";
        prev.lpi = 0;
        prev.fsNodes = [];
        return {...prev};
      });

      return;
    }
    refreshPages(false);
    
  }, [props.albumCm]);

  const refreshPages = (restartPage: boolean) => {
    if(!props.albumCm) return;

    setLoading(true);

    const includeDetail = apm.detailLevel === 'full';

    axiosA.get<AlbumFsNodeInfo>(_uri.GetAlbumFsNodeInfo(props.type, props.albumCm.path, includeDetail, includeDetail))
      .then(function (response) {
        setApm({
          path: props.albumCm!.path,
          lpi: restartPage ? 0 : props.albumCm!.lastPageIndex,
          orientation: response.data.orientation,
          fsNodes: response.data.fsNodes,
          indexes: _constant.defaultIndexes,
          detailLevel: apm.detailLevel,
        });

        if (props.albumCm!.pageCount !== _helper.countFileNodes(response.data.fsNodes) && props.type !== 1) {
          axiosA.get<string>(_uri.RefreshAlbum(props.type, props.albumCm!.path))
            .then(function (response) {
            })
            .catch(function (error) {
              notif.apiError(error);
            });
        }
      })
      .catch(function (error) {
        notif.apiError(error);
      })
      .finally(() => setLoading(false));
  }

  useEffect(() => {
    const fileCount = _helper.countFileNodes(apm.fsNodes);
    const targetPage = apm.lpi < fileCount ? apm.lpi : fileCount - 1;

    navHandler.jumpTo(targetPage);
  }, [apm.path]);

  useEffect(() => {
    document.addEventListener("keydown", handleKeyDown2, false);

    return () => { //component will unmount
      document.removeEventListener("keydown", handleKeyDown2, false);
    };
  }, []);

  const handleKeyDown2 = (event: any) => {
    if (event.keyCode === 27 && props.albumCm != null) {
      navHandler.close();
    }
  };

  //#region Event handlers
  const navHandler = {
    jumpTo: function (page: number) {
      const fileCount = _helper.countFileNodes(apm.fsNodes); 

      const maxIndex = fileCount - 1;
      const newPPageI = apm.indexes?.cPageI ?? 0;
      const newCPageI = _helper.clamp(page, 0, maxIndex);

      const { newSlide, slideAIndex, slideBIndex, slideCIndex } = ((): {
        newSlide: string, slideAIndex: number, slideBIndex: number, slideCIndex: number
      } => {
        function pageCeil(modifier: number) {
          const pagesLen = fileCount;
          return (((newCPageI + modifier) % pagesLen) + pagesLen) % pagesLen;
        }

        if (newCPageI % 3 === 0) {
          return {
            newSlide: "slideA",
            slideAIndex: newCPageI,
            slideBIndex: pageCeil(1),
            slideCIndex: pageCeil(-1)
          }
        }
        if (newCPageI % 3 === 1) {
          return {
            newSlide: "slideB",
            slideAIndex: pageCeil(-1),
            slideBIndex: newCPageI,
            slideCIndex: pageCeil(1)
          }
        }
        
        return {
          newSlide: "slideC",
          slideAIndex: pageCeil(1),
          slideBIndex: pageCeil(-1),
          slideCIndex: newCPageI
        }
      })();

      setApm({
        indexes: {
          cPageI: newCPageI,
          pPageI: newPPageI,
          cSlide: newSlide,
          slideAIndex: slideAIndex,
          slideBIndex: slideBIndex,
          slideCIndex: slideCIndex
        },
        path: apm.path,
        lpi: apm.lpi,
        orientation: apm.orientation,
        detailLevel: apm.detailLevel,
        fsNodes: apm.fsNodes
      });

      if (apm.path !== "" && page === fileCount - 1 && props.type !== 1) {
        axiosA.post(_uri.UpdateAlbumOuterValue(props.type), {
          albumPath: apm.path,
          lastPageIndex: page
        })
          .then((response) => {
          })
          .catch((error) => {
            notif.apiError(error);
          });
      }
    },
    close: () => {
      props.onClose(apm.path, apm.indexes.cPageI);
    },
  };

  const pageHandler = {
    rename: (newVal: string, oldVal: string) => {
      const srcAlRelPath = currentPageInfo.libRelPath;
      const dstAlRelPath = (() => {
        const arr = currentPageInfo.libRelPath.split('\\');
        arr.pop();
        arr.push(newVal);

        return _helper.pathJoin(arr, '\\');
      })();

      const movObj = {
        overwrite: false,
        src: {
          albumPath: apm.path,
          alRelPath: srcAlRelPath,
        },
        dst: {
          albumPath: apm.path,
          alRelPath: dstAlRelPath
        }
      };

      pageManager.movePage(props.type, movObj, (response) => {
          setApm((prev) => {
            const isInRoot = prev.fsNodes.findIndex(a => a.alRelPath === srcAlRelPath) !== -1;
            if(isInRoot){
              prev.fsNodes = prev.fsNodes.filter(a => a.alRelPath !== dstAlRelPath); //Remove overwritten node

              const foundIdx = prev.fsNodes.findIndex(a => a.alRelPath === srcAlRelPath);
              if(foundIdx !== -1){
                prev.fsNodes[foundIdx].alRelPath = dstAlRelPath;
                prev.fsNodes[foundIdx].fileInfo!.name = newVal;
                prev.fsNodes[foundIdx].fileInfo!.libRelPath = dstAlRelPath;
              }
            }
            else{
              prev.fsNodes.forEach(a => {
                if(a.nodeType !== NodeType.Folder) return;

                const isInCurrentChapter = a.dirInfo!.childs.findIndex(b => b.alRelPath === srcAlRelPath) !== -1;
                if(isInCurrentChapter){
                  a.dirInfo!.childs = a.dirInfo!.childs.filter(b => b.alRelPath !== dstAlRelPath); //Remove overwritten node
                  
                  const foundIdx = a.dirInfo!.childs.findIndex(b => b.alRelPath === srcAlRelPath);
                  if(foundIdx !== -1){
                    a.dirInfo!.childs[foundIdx].alRelPath = dstAlRelPath;
                    a.dirInfo!.childs[foundIdx].fileInfo!.name = newVal;
                    a.dirInfo!.childs[foundIdx].fileInfo!.libRelPath = dstAlRelPath;
                  }
                }
              });
            }

            return ({
              ...prev,
              fsNodes: [...prev.fsNodes]
            });
          });
        },
        (error) => {
          notif.apiError(error);
        }
      )
    },
    move: (newDir: string) => {
      //Currently at 2023-05-21
      //This function only applies for SC
      const srcAlRelPath = currentPageInfo.libRelPath;

      const movObj = {
        overwrite: false,
        src: {
          albumPath: apm.path,
          alRelPath: srcAlRelPath,
        },
        dst: {
          albumPath: newDir,
          alRelPath: currentPageInfo.name
        }
      };

      pageManager.movePage(props.type, movObj, 
        (response) => {
          removePageFromApm(currentPageInfo.libRelPath);
        },
        (error) => {
          notif.apiError(error);
        }
      )
    },
    delete: (pageInfo: FileInfoModel, directDelete: boolean) => {
      const delObj = {
        albumPath: apm.path,
        alRelPath: pageInfo.libRelPath
      };

      pageManager.deletePage(props.type, delObj, directDelete, (response) => {
          removePageFromApm(currentPageInfo.libRelPath);
        },
        (error: any) => {
          notif.apiError(error);
        });
    },
    chapterDeleteSuccess: (chapterName: string) => {
      const newFsNodes = apm.fsNodes.filter(a => a.dirInfo?.name !== chapterName);
      setApm(prev => {
        prev.fsNodes = newFsNodes;
        prev.indexes = _constant.defaultIndexes;
        return {...prev};
      });

      props.onChapterDeleteSuccess(apm.path, _helper.countFileNodes(newFsNodes));
    },
    chapterRenameSuccess: (chapterName: string, newChapterName: string) => {
      const newFsNodes = apm.fsNodes;
      const fsNodeIdx = newFsNodes.findIndex(a => a.dirInfo?.name === chapterName);
      newFsNodes[fsNodeIdx].alRelPath = `${newChapterName}`;
      newFsNodes[fsNodeIdx].dirInfo!.name = newChapterName;
      newFsNodes[fsNodeIdx].dirInfo!.childs = newFsNodes[fsNodeIdx].dirInfo!.childs.map(a => {
        return {
          ...a,
          alRelPath: `${newChapterName}\\${a.fileInfo!.name}`,
          fileInfo: {
            ...a.fileInfo!,
            libRelPath: `${newChapterName}\\${a.fileInfo!.name}`
          }
        }
      });

      setApm(prev => {
        return {
          ...prev,
          fsNodes: [...newFsNodes]
        };
      });
    },
    chapterTierChangeSuccess: (chapterName: string, tier: number) => {
      const newFsNodes = apm.fsNodes;
      const fsNodeIdx = newFsNodes.findIndex(a => a.dirInfo?.name === chapterName);
      newFsNodes[fsNodeIdx].dirInfo!.tier = tier;

      setApm(prev => {
        return {
          ...prev,
          indexes: clampIndexes(prev.indexes, _helper.countFileNodes(newFsNodes) - 1),
          fsNodes: [...newFsNodes]
        };
      });
    }
  }

  function removePageFromApm(alRelPath: string){
    setApm(prev => {
      prev.fsNodes = prev.fsNodes.filter(a => a.fileInfo?.libRelPath !== alRelPath);
      prev.fsNodes.forEach(a => {
        if(a.nodeType !== NodeType.Folder) return;

        a.dirInfo!.childs = a.dirInfo!.childs.filter(b => b.fileInfo?.libRelPath !== alRelPath);
      });

      return{
        ...prev,
        indexes: clampIndexes(prev.indexes, _helper.countFileNodes(prev.fsNodes) - 1),
        fsNodes: [...prev.fsNodes]
      }
    });
  }

  function clampIndexes(indexes: PagingIndex, max: number): PagingIndex{
    return{
      cPageI:_helper.clamp(indexes.cPageI, 0, max),
      pPageI:_helper.clamp(indexes.pPageI, 0, max),
      slideAIndex:_helper.clamp(indexes.slideAIndex, 0, max),
      slideBIndex:_helper.clamp(indexes.slideBIndex, 0, max),
      slideCIndex:_helper.clamp(indexes.slideCIndex, 0, max),
      cSlide: indexes.cSlide
    }
  }

  function changeDetailVisibility(){
    const nextDetailLevel = apm.detailLevel == 'none' ? 'simple' 
      : apm.detailLevel == 'simple' ? 'full' 
      : 'none';

    const nextIncludeDetail = nextDetailLevel === 'full';

    if(nextIncludeDetail && pages[0].createDate == null){
      setLoading(true);

      axiosA.get<AlbumFsNodeInfo>(_uri.GetAlbumFsNodeInfo(props.type, props.albumCm!.path, nextIncludeDetail, nextIncludeDetail))
      .then(function (response) {
        setApm(prev => {
          prev.detailLevel = nextDetailLevel;
          prev.fsNodes = response.data.fsNodes;
          return { ...prev };
        });
      })
      .catch(function (error) {
        notif.apiError(error);
      })
      .finally(() => setLoading(false));
    }
    else{
      setApm(prev => {
        prev.detailLevel = nextDetailLevel;
        return { ...prev };
      });
    }
  }

  const albumHandler = {
    tierChange: (value: number) => {
      axiosA.put(_uri.UpdateAlbumTier(props.type), {
        albumPath: apm.path,
        tier: value
      })
        .then(function (response) {
        })
        .catch(function (error) {
          notif.apiError(error);
        });
    },
    recount: () => {
      axiosA.get<number>(_uri.RecountAlbumPages(props.type, apm.path))
        .then(function (response) {
          props.onChapterDeleteSuccess(apm.path, response.data);
        })
        .catch(function (error) {
          notif.apiError(error);
        });
    }
  }

  const [showContextMenu, setShowContextMenu] = useState(false);
  const [showDrawer, setShowDrawer] = useState(false);

  if (loading){ return (<Spinner loading={true} />); }
  if (apm.indexes === null || pages.length === 0) { return (<></>); }

  const currentPageInfo = apm.indexes.cSlide === "slideA" ? pages[apm.indexes.slideAIndex]
    : apm.indexes.cSlide === "slideB" ? pages[apm.indexes.slideBIndex]
      : pages[apm.indexes.slideCIndex];

  const getPageSrc = (pageInfo: FileInfoModel) => setting.directFileAccess 
    ? (`${props.type === 0 ? '_hlibrary' : '_hsc'}\\${apm.path}\\${pageInfo.libRelPath}`)
      .replace('%', '%25').replace('#', '%23').replace('+', '%2B')
    : _uri.StreamPage(`${apm.path}\\${pageInfo.libRelPath}`, props.type);
  
    
  //2024-10-10 Rotation status
  //If the screen is landscape, the reader is never rotated
  //Ocr mode is always upright and doesn't respect rotateReader. TODO: Ocr mode rotation
  //Auto orientation is treated as portrait. TODO: Page-level rotation

  const rotateReader = (() =>{
    if(setting.alwaysPortrait)
      return false;

    if(!IsScreenPortrait())
      return false;

    return apm.orientation === 'Landscape';
  })();
  
  const { transformStyle, vwvhStyle } = _helper.getStyleByRotation2(rotateReader);

  if(ocrMode){
    return(
      <>
        <div className=' background-blackout-2 z-10'></div>
        <div className=' fixed left-0 top-0 z-20'>
          <TranslateDisplay 
            apm={apm}
            pageInfo={currentPageInfo}
            getPageSrc={getPageSrc}
            onExit={() => setOcrMode(false)}
          />
        </div>
      </>
    )
  }

  return (
    <>
      <div className={` background-blackout z-10`}></div>
      <div className=' fixed left-0 top-0 z-20' style={{...transformStyle, ...vwvhStyle}}>
        <ReaderDisplay 
          apm={apm}
          pages={pages}
          setting={setting}
          currentPageInfo={currentPageInfo}
          getPageSrc={getPageSrc}
        />
        <Overlay3x3Nav 
          rotateReader={rotateReader}
          showGuide={false}

          onClose={navHandler.close}
          onNext={() => navHandler.jumpTo(apm.indexes.cPageI + 1)}
          onPrev={() => navHandler.jumpTo(apm.indexes.cPageI - 1)}
          onJumpToFirst={() => navHandler.jumpTo(0)}
          onDelete={() => pageHandler.delete(currentPageInfo, true)}

          onOpenEditModal={props.onOpenEditModal}
          onShowContextMenu={() => setShowContextMenu(true)}
          onShowDrawer={() => setShowDrawer(true)}
          onChangeDetailVisibility={changeDetailVisibility}
          onChangeOcrMode={() => {
            setOcrMode(!ocrMode);
          }}
        />
        {/* props.albumCm is checked here instead of in Albums or SelfComps is for performance reason. When ReaderModal is closed and opened again with the same album, it doesn't fetch the pages again from backend*/}
        {props.albumCm && 
          <>
            <ReaderModalContextMenu
              visible={showContextMenu}
              albumCm={props.albumCm}
              initialValue={apm.indexes.cPageI}
              pageName={currentPageInfo.name}
              type={props.type}
      
              onTierChange={albumHandler.tierChange}
              onRecount={() => { albumHandler.recount(); setShowContextMenu(false); }}
              onJump={(page) => { navHandler.jumpTo(page); setShowContextMenu(false); }}
              onJumpToLast={() => { navHandler.jumpTo(pages.length - 1); setShowContextMenu(false); }}
              onUndoJump={() => { navHandler.jumpTo(apm.indexes?.pPageI ?? 0); setShowContextMenu(false); }}
              onRename={(newVal, oldVal) => { pageHandler.rename(newVal, oldVal); setShowContextMenu(false); }}
              onMove={(newDir) => {
                if(!newDir) return;
                pageHandler.move(newDir);
                setShowContextMenu(false); 
              }}
              onDelete={() => { pageHandler.delete(currentPageInfo, false); setShowContextMenu(false); }}
              onCommitDelete={() => { pageHandler.delete(currentPageInfo, true); setShowContextMenu(false); }}
              onCancel={() => setShowContextMenu(false)}
              onArtistClick={(artist: string) => { 
                navigate('/Albums?query=artist='+artist);
                navHandler.close();
              }}
            />
            <ChapterDrawer
              visible={showDrawer}
              albumPath={apm.path}
              fsNodes={apm.fsNodes}
              currentPageIndex={apm.indexes.cPageI}
              type={props.type}

              onClose={() => setShowDrawer(false)}
              onJumpToPage={navHandler.jumpTo}
              onChapterDeleteSuccess={pageHandler.chapterDeleteSuccess}
              onChapterRenameSuccess={pageHandler.chapterRenameSuccess}
              onChapterTierChangeSuccess={pageHandler.chapterTierChangeSuccess}
            />
          </>
        }
      </div>
    </>
  );
}