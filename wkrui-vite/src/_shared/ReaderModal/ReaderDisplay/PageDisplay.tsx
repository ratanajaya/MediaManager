import { FileInfoModel, SettingVal } from '_utils/Types';
import * as CSS from 'csstype';
import _helper from '_utils/_helper';
import loadingImg from '_assets/resources/loading.gif';
import { useRef, useState } from 'react';
import { Progress } from 'antd';

export default function PageDisplay(props: {
  id: string,
  albumPath: string,
  pageInfo: FileInfoModel,
  queue: string,
  setting: SettingVal,
  getPageSrc: (coverInfo: FileInfoModel) => string,
}) {
  const styleShow: CSS.Properties = {
    width: "100%",
    height: "100%"
  };
  const styleHide: CSS.Properties = { 
    height: 0, 
    width: 0,
  };
  
  return (
    <div style={{
      position: 'relative',
      ...(props.queue === "q0" ? styleShow : styleHide)
    }}>
      {_helper.isVideo(props.pageInfo.extension) ? 
        (props.queue === "q0" ?
          <VideoPlayer
            src={props.getPageSrc(props.pageInfo)}
            isMuted={props.setting.muteVideo} 
          />
          : null)
        : (
        <img
          id={props.id}
          style={{height:'100%', width:'100%', objectFit:'contain'}}
          src={props.queue === "q0" || props.queue === "q1"
            ? props.getPageSrc(props.pageInfo)
            : loadingImg
          }
          alt="Not loaded"
        />
      )}
    </div>
  );
}

function VideoPlayer (props: {
  src: string,
  isMuted: boolean,
}) {
  const videoRef = useRef<HTMLVideoElement | null>(null);
  const [progress, setProgress] = useState<number>(0);

  const handleTimeUpdate = () => {
    const video = videoRef.current;
    if (video) {
      const currentTime = video.currentTime;
      const duration = video.duration;
      if (duration > 0) {
        const progressPercentage = (currentTime / duration) * 100;
        setProgress(progressPercentage);
      }
    }
  };

  const handleSeek = (value: number) => {
    const video = videoRef.current;
    if (video) {
      const seekTime = (value / 100) * video.duration;
      video.currentTime = seekTime;
    }
  };

  return (
    <>
      <video
        ref={videoRef}
        autoPlay
        loop
        muted={false}
        src={props.src}
        style={{ objectFit: 'contain', width: '100%', height: '100%' }}
        onTimeUpdate={handleTimeUpdate}
      >
        video not showing
      </video>
      <div style={{ position: 'absolute', bottom: '0', left: '0', width: '100%' }}>
        <Progress
          percent={progress}
          showInfo={false}
          size="small"
          strokeColor={"#113049"}
        />
      </div>
    </>
  );
}