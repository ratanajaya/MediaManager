import { FileInfoModel, FsNode, SettingVal } from '_utils/Types';
import PageDisplay from './PageDisplay';
import _helper from '_utils/_helper';
import OverlayPageInfo from './OverlayPageInfo';

export interface PagingIndex{
  cPageI: number,
  pPageI: number,
  cSlide: string,
  slideAIndex: number,
  slideBIndex: number,
  slideCIndex: number,
}

export interface AlbumPaginationModel{
  path: string,
  lpi: number,
  fsNodes: FsNode[],
  indexes: PagingIndex,
  orientation: 'Portrait' | 'Landscape' | 'Auto',
  detailLevel: 'none' | 'simple' | 'full',
}

export default function ReaderDisplay(props: {
  apm: AlbumPaginationModel,
  pages: FileInfoModel[],
  setting: SettingVal,
  currentPageInfo: FileInfoModel,
  getPageSrc: (pageInfo: FileInfoModel) => string,
}){
  const { apm, pages, setting, getPageSrc } = props;

  const slideAPage = <PageDisplay
    id="page-a"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.slideAIndex]}
    queue={apm.indexes.cSlide === "slideA" ? "q0" : apm.indexes.cSlide === "slideC" ? "q1" : "q2"}
    setting={setting}
    getPageSrc={getPageSrc}
  />;

  const slideBPage = <PageDisplay
    id="page-b"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.slideBIndex]}
    queue={apm.indexes.cSlide === "slideB" ? "q0" : apm.indexes.cSlide === "slideA" ? "q1" : "q2"}
    setting={setting}
    getPageSrc={getPageSrc}
  />;

  const slideCPage = <PageDisplay
    id="page-b"
    albumPath={apm.path}
    pageInfo={pages[apm.indexes.slideCIndex]}
    queue={apm.indexes.cSlide === "slideC" ? "q0" : apm.indexes.cSlide === "slideB" ? "q1" : "q2"}
    setting={setting}
    getPageSrc={getPageSrc}
  />;

  return (
    <>
      {slideAPage}
      {slideBPage}
      {slideCPage}
      <OverlayPageInfo 
        apm={apm}
        currentPageInfo={props.currentPageInfo}
        totalPages={pages.length}
      />
    </>
  );
}