import { useSetting } from '_shared/Contexts/SettingProvider';
import ReaderDisplay, { AlbumPaginationModel } from '_shared/ReaderModal/ReaderDisplay';
import Overlay3x3Nav from '_shared/ReaderModal/Overlay3x3Nav';
import Spinner from '_shared/Spinner';
import { FileInfoModel, OfflineAlbumCm, OfflineFile } from '_utils/Types';
import _constant from '_utils/_constant';
import _db from '_utils/_db';
import _helper from '_utils/_helper';
import { useEffect, useMemo, useState } from 'react';
import OfflineChapterDrawer from './OfflineChapterDrawer';
import { IsScreenPortrait } from '_utils/_display';

export default function OfflineReaderModal(props:{
  albumCm: OfflineAlbumCm,
  onClose: (path: string, lpi: number) => void
}) {
  const [apm, setApm] = useState<AlbumPaginationModel>({
    path: props.albumCm.path,
    lpi: props.albumCm.lastPageIndex,
    fsNodes: props.albumCm.fsNodes,
    indexes: _constant.defaultIndexes,
    orientation: 'Portrait',
    detailLevel: 'none',
  });
  
  const { setting } = useSetting();

  const [loading, setLoading] = useState<boolean>(false);
  
  const pages = useMemo(() => {
    return _helper.getFlatFileInfosFromFsNodes(apm.fsNodes);
  }, [apm.fsNodes]);

  const [offlineFiles, setOfflineFiles] = useState<OfflineFile[]>([]);

  useEffect(() => {
    setLoading(true);
    _db.offlineFiles.filter(e => e.albumPath === apm.path).toArray()
      .then((resOfflineFiles) => {
        setOfflineFiles(resOfflineFiles);
      })
      .catch((err) => console.error(err))
      .finally(() => setLoading(false));

    setLoading(false);
  }, []);

  const navHandler = {
    jumpTo: function (page: number) {
      const newIndexes = _helper.getIndexes(page, pages.length, apm.indexes.cPageI);

      setApm(prev => { 
        return{
          ...prev,
          indexes: newIndexes
      }});
    },
    close: () => {
      props.onClose(apm.path, apm.indexes.cPageI);
    },
  }

  const [showDrawer, setShowDrawer] = useState(false);

  if (loading){ return (<Spinner loading={true} />); }
  if (apm.indexes === null || pages.length === 0) { return (<></>); }

  const currentPageInfo = apm.indexes.cSlide === "slideA" ? pages[apm.indexes.slideAIndex]
    : apm.indexes.cSlide === "slideB" ? pages[apm.indexes.slideBIndex]
      : pages[apm.indexes.slideCIndex];

  const getPageSrcFromUncPath = (pageUncPath: string) => {
    if(offlineFiles.length === 0)
      return _constant.imgPlaceholder;

    const coverAlRelPath = pageUncPath.replace(`${apm.path}\\`, '');
    const offlineFile = offlineFiles.find(e => e.albumPath === apm.path && e.alRelPath === coverAlRelPath);

    if(offlineFile){
      return URL.createObjectURL(offlineFile.blob);
    }

    return _constant.imgPlaceholder;
  }

  const getPageSrcFromFileInfo = (fileInfo: FileInfoModel) => {
    return getPageSrcFromUncPath(fileInfo.libRelPath);
  }

  const rotateReader = (() =>{
    if(setting.alwaysPortrait)
      return false;

    if(!IsScreenPortrait())
      return false;

    return apm.orientation === 'Landscape';
  })();

  const { transformStyle, vwvhStyle } = _helper.getStyleByRotation2(rotateReader);
  
  return (
    <>
      <div className='background-blackout z10'></div>
      <div className=' fixed left-0 top-0 z-20' style={{...transformStyle, ...vwvhStyle}}>
        <ReaderDisplay 
          apm={apm}
          pages={pages}
          setting={setting}
          currentPageInfo={currentPageInfo}
          getPageSrc={getPageSrcFromFileInfo}
        />
        <Overlay3x3Nav 
          rotateReader={rotateReader}
          showGuide={false}

          onClose={navHandler.close}
          onNext={() => navHandler.jumpTo(apm.indexes.cPageI + 1)}
          onPrev={() => navHandler.jumpTo(apm.indexes.cPageI - 1)}
          onJumpToFirst={() => navHandler.jumpTo(0)}
          onDelete={() => {}}

          onOpenEditModal={() => {}}
          onShowContextMenu={() => {}}
          onShowDrawer={() => setShowDrawer(true)}
          onChangeDetailVisibility={() => {
            setApm(prev => {
              return {
                ...prev,
                detailLevel: prev.detailLevel === 'none' ? 'simple' : 'none'
              }
            })
          }}
          onChangeOcrMode={() => {}}
        />
        <OfflineChapterDrawer 
          albumPath={apm.path}
          fsNodes={apm.fsNodes}
          visible={showDrawer}
          currentPageIndex={apm.indexes.cPageI}
          onClose={() => setShowDrawer(false)}
          onJumpToPage={(pageIndex) => navHandler.jumpTo(pageIndex)}
          getCoverPageSrc={getPageSrcFromUncPath}
        />
      </div>
    </>
  )
}