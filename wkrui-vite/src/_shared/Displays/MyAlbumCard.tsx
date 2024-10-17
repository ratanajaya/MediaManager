import { Dropdown } from 'antd';
import { useInView } from 'react-intersection-observer';
import MyFlagIcon from '_shared/Displays/MyFlagIcon';
import { AlbumCardModel, FileInfoModel } from '_utils/Types';
import _helper from '_utils/_helper';

function MyAlbumCard(props : {
  albumCm: AlbumCardModel,
  onView: (albumCm: AlbumCardModel) => void,
  onEdit?: (path: string) => void,
  showContextMenu: boolean,
  showPageCount?: boolean,
  showDate?: boolean,
  getCoverSrc: (coverInfo: FileInfoModel) => string,
}) {
  const handler = {
    view: function () {
      props.onView(props.albumCm)
    },

    edit: function () {
      if(props.onEdit) props.onEdit(props.albumCm.path);
    },
  }

  const [ref, inView, entry] = useInView({
    threshold: 0,
    rootMargin: "300px 0px 300px 0px"
  });
  
  return (
    <div ref={ref} className='my-album-card-container-1'>
      <div className='my-album-card-container-2'>
        <Dropdown 
          menu={{ items: [{key: '1',label: <></>}], style: {display:'none'} }} 
          onOpenChange={(open) => { if(open) handler.edit(); } } trigger={['contextMenu']} 
          destroyPopupOnHide={true} disabled={!props.showContextMenu}
        >
        <div className='my-album-card-container-3' onClick={handler.view}>
          {inView && 
            <img className='my-album-card-img' alt="img"
              src={props.getCoverSrc(props.albumCm.coverInfo)}
            />
          }

          <div className='my-album-card-flag'>
            <div style={{flex:"4", display:"flex"}}>
              {!props.albumCm.isRead &&
                <MyFlagIcon flagType={"New"} />
              }
              {props.albumCm.isWip &&
                <MyFlagIcon flagType={"Wip"} />
              }
              {props.albumCm.hasSource &&
                <MyFlagIcon flagType={"Source"} />
              }
            </div>
            <div style={{flex:'6', display:'inline-flex', justifyContent:'end'}}>
              {props.albumCm.languages.map((item, i) => <MyFlagIcon flagType={item} key={i} />)}
            </div>
          </div>
          
          {props.showPageCount &&
            <div className="album-pagecount">
              {props.albumCm.pageCount}
            </div>
          }

          <div className='my-album-card-tierbar'>
            {[3, 2, 1].map((e, i) =>
              <div key={`tierBar-${i}`}
                style={{
                  width: "100%",
                  height: "33%",
                  backgroundColor: _helper.ColorFromIndex(props.albumCm.tier, e),
                  borderTop: _helper.BorderFromIndex(props.albumCm.tier, e),
                  borderRight: _helper.BorderRightFromIndex(props.albumCm.tier, e)
                }} />
            )}
          </div>

          <div className='my-album-card-progressbar' style={{ display: props.albumCm.lastPageIndex > 0 ? "block" : "none" }}>
            <div style={{
              backgroundColor: "DodgerBlue",
              height: "100%",
              width: `${_helper.getPercent100(props.albumCm.lastPageIndex + 1, props.albumCm.pageCount)}%`
            }} />
          </div>

          {(props.albumCm.note !== null && props.albumCm.note !== "") &&
            <div className="album-note">
              {props.albumCm.note}
            </div>
          }

          <div>
          </div>
        </div>
        </Dropdown>
      </div>
      <div className='pl-3 pr-3 pt-1 mb-2'>
        {props.showDate &&
          <div className=' mb-1 text-left'>
            {_helper.formatNullableDatetime2(props.albumCm.entryDate)}
          </div>
        }
        {props.albumCm.artistDisplay ?
          <>
            <div className='my-album-card-title'>
              {props.albumCm.title}
            </div>
            <div className='divider-4'></div>
            <div className='my-album-card-artist'>
              {props.albumCm.artistDisplay}
            </div> 
          </> :
          <div className='my-album-card-general'>
            {props.albumCm.title}
          </div>
        }
      </div>
    </div>
  );
}

export default MyAlbumCard;