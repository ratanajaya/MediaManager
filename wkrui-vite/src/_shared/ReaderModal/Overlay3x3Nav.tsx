import { Dropdown } from 'antd';
import { SwipeEventData, useSwipeable } from 'react-swipeable';

export default function Overlay3x3Nav(props: {
  showGuide: boolean,
  rotateReader: boolean,
  onClose: () => void,
  onNext: () => void,
  onPrev: () => void,
  onJumpToFirst: () => void,
  onDelete: () => void,

  onOpenEditModal: () => void,
  onShowContextMenu: () => void,
  onShowDrawer: () => void,
  onChangeDetailVisibility: () => void,
  onChangeOcrMode: () => void,
}) {
  const swipeHandlerEvents ={
    onSwipedUp: (e: SwipeEventData) => { 
      if(!props.rotateReader) return;
      props.onPrev();
    },
    onSwipedDown: (e: SwipeEventData) => { 
      if(!props.rotateReader) return;
      props.onNext();
    },
    onSwipedLeft: (e: SwipeEventData) => { 
      if(props.rotateReader) return;
      props.onNext();
    },
    onSwipedRight: (e: SwipeEventData) => { 
      if(props.rotateReader) return; 
      props.onPrev();
    }
  };
  
  const swipeHandler1 = useSwipeable(swipeHandlerEvents);
  const swipeHandler2 = useSwipeable(swipeHandlerEvents);
  const swipeHandler3 = useSwipeable(swipeHandlerEvents);

  const bottomSwipeHandler = useSwipeable({
    onSwipedRight: (eventData) => {
      const swipeLen = Math.max(Math.abs(eventData.deltaX), Math.abs(eventData.deltaY));
      const shouldDelete = swipeLen > 100 && eventData.velocity > 0.5;

      if(shouldDelete){
        props.onDelete();
      }
    },
    onSwipedUp: (eventData) => { 
      const swipeLen = Math.max(Math.abs(eventData.deltaX), Math.abs(eventData.deltaY));
      const shouldDelete = swipeLen > 100 && eventData.velocity > 0.5;

      if(shouldDelete){
        props.onDelete();
      }
    },
  });

  const buttonHandler = {
    rightTop: props.onClose,
    rightMid: props.onNext,
    rightBot: props.onChangeOcrMode,
    midTop: props.onChangeDetailVisibility,
    midMid: props.onShowContextMenu,
    midBot: () => {},
    leftTop: props.onShowDrawer,
    leftMid: props.onPrev,
    leftBot: props.onJumpToFirst,
  }

  const guideBorderClass = props.showGuide ? "border-2 border-gray-300 border-collapse" : "";

  return (
    <div className='overlay' style={{ zIndex: 4, height:'100%', width: '100%' }}>
      <div className="checker-row" style={{height:'33%'}} {...swipeHandler1}>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.leftTop} >
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.midTop}>
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.rightTop}>
        </div>
      </div>
      <div className="checker-row" style={{height:'33%'}} {...swipeHandler2}>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.leftMid}>
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.midMid}>
          <Dropdown 
            menu={{ items: [{key: '1',label: <></>}], style: {display:'none'} }} 
            onOpenChange={(open) => { if(open) props.onOpenEditModal(); } } trigger={['contextMenu']} 
            destroyPopupOnHide={true}
          >
            <div style={{height:'100%', width:'100%'}}>
            </div>
          </Dropdown>
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.rightMid}>
        </div>
      </div>
      <div className="checker-row" style={{height:'28%'}} {...swipeHandler3}>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.leftBot}>
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.midBot}>
        </div>
        <div className={`checker ${guideBorderClass}`} onClick={buttonHandler.rightBot} >
        </div>
      </div>
      <div className="checker-row" style={{height:'6%'}} {...bottomSwipeHandler}>
      </div>
    </div>
  )
}
