import { AlbumPaginationModel } from ".";
import { FileInfoModel } from '_utils/Types';
import _helper from '_utils/_helper';

export default function OverlayPageInfo(props: {
  apm: AlbumPaginationModel,
  currentPageInfo: FileInfoModel,
  totalPages: number,
}){
  return(
    <div className="overlay" style={{ zIndex: 3, height: '100%', width: '100%' }}>
      {['simple', 'full'].includes(props.apm.detailLevel) && 
        <div className="with-shadow w-full flex">
          <div className=" flex-1"></div>
          <div className=" flex-1 text-center">{props.currentPageInfo.name}</div>
          <div className=" flex-1 text-right pr-2">
            {props.apm.detailLevel === 'full' &&
              <>
                <div>{_helper.formatBytes(props.currentPageInfo.size, 0)}</div>
                <div>{props.currentPageInfo.width} x {props.currentPageInfo.height}</div>
              </>
            }
          </div>
        </div>
      }
      <div style={{ position: "fixed", bottom: "0px", width: "100%", textAlign: "center" }}>
        <span className="with-shadow">{props.apm.indexes.cPageI + 1}/{props.totalPages}</span>
      </div>
    </div>
  )
}