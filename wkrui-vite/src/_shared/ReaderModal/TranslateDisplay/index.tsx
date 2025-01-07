import { useRef, useState } from 'react'
import { AlbumPaginationModel } from '../ReaderDisplay'
import { FileInfoModel, OcrResult } from '_utils/Types'
import ReactCrop, { Crop } from 'react-image-crop'
import 'react-image-crop/dist/ReactCrop.css'
import _uri from '_utils/_uri'
import { useAuth } from '_shared/Contexts/useAuth'
import { LocalSpinner } from '_shared/Spinner'
import _helper from '_utils/_helper'

//2024-10-10 Currently the OCR mode is always upright
export default function TranslateDisplay(props:{
  apm: AlbumPaginationModel,
  pageInfo: FileInfoModel,
  getPageSrc: (pageInfo: FileInfoModel) => string,
  onExit: () => void,
}) {
  const [loading, setLoading] = useState(false);
  const [crop, setCrop] = useState<Crop>();
  const [ocrResultDisplay, setOcrResultDisplay] = useState<{
    ocrResult: OcrResult,
    location: {
      x: number,
      y: number,
    }
  } | null>(null);

  const imgRef = useRef<HTMLImageElement>(null);
  const divRef = useRef<HTMLDivElement>(null);

  const { axiosA } = useAuth();
  
  const handleCompleteCrop = (crop: Crop) => {
    if (!crop || !imgRef.current) return;
    const img = imgRef.current;

    // Get container and image dimensions
    const containerWidth = img.clientWidth;
    const containerHeight = img.clientHeight;
    const naturalWidth = img.naturalWidth;
    const naturalHeight = img.naturalHeight;

    // Calculate actual displayed image dimensions (accounting for object-fit: contain)
    const imageAspectRatio = naturalWidth / naturalHeight;
    const containerAspectRatio = containerWidth / containerHeight;
    
    const [ renderedHeight, renderedWidth ] = imageAspectRatio > containerAspectRatio 
      ? [ containerWidth / imageAspectRatio, containerWidth ]
      : [ containerHeight, containerHeight * imageAspectRatio ];

    // Calculate letterboxing offsets
    const xOffset = (containerWidth - renderedWidth) / 2;
    const yOffset = (containerHeight - renderedHeight) / 2;

    // Calculate scaling factors
    const scaleX = naturalWidth / renderedWidth;
    const scaleY = naturalHeight / renderedHeight;

    // Create canvas with crop dimensions
    const canvas = document.createElement('canvas');
    canvas.width = crop.width;
    canvas.height = crop.height;
    
    const ctx = canvas.getContext('2d');
    if (!ctx) return;

    // Calculate actual crop coordinates in original image space
    const sourceX = (crop.x - xOffset) * scaleX;
    const sourceY = (crop.y - yOffset) * scaleY;
    const sourceWidth = crop.width * scaleX;
    const sourceHeight = crop.height * scaleY;

    ctx.drawImage(
      img,
      sourceX,
      sourceY,
      sourceWidth,
      sourceHeight,
      0,
      0,
      crop.width,
      crop.height
    );

    // Convert canvas to a blob (you could also use canvas.toDataURL() if needed)
    canvas.toBlob((blob) => {
      if(!blob) return;

      setLoading(true);
      
      const formData = new FormData();
      formData.append('file', blob, 'image.jpg');
      axiosA.post<OcrResult>(_uri.TranscribeAndTranslateImage(), formData, {})
        .then((res) => {
          const divRect = divRef.current!.getBoundingClientRect();
          const imgRect = imgRef.current!.getBoundingClientRect();
          const distanceFromLeft = imgRect.left - divRect.left;
          const distanceFromTop = imgRect.top - divRect.top;

          setOcrResultDisplay({
            ocrResult: res.data, 
            location:{
              x: crop.x + distanceFromLeft,
              y: crop.y + distanceFromTop
            }
          });
        })
        .catch((err) => {
          console.error(err)
        })
        .finally(() => setLoading(false));
        
    }, 'image/jpeg');
  };

  return (
    <div 
      ref={divRef}
      className='h-full w-full flex justify-center items-center'//fixed z-[2] left-0 top-0 
    >
      {ocrResultDisplay && (
        <span 
          className=' bg-white text-black font-semibold pl-1 pr-1 border-gray-200 border-2 rounded-sm fixed z-10 max-w-64' 
          style={{ top: ocrResultDisplay.location.y, left: ocrResultDisplay.location.x }}
        >
          {_helper.decodeHTMLEntities(ocrResultDisplay?.ocrResult.english)}
        </span>
      )}
      <LocalSpinner loading={loading}>
        <ReactCrop
          crop={crop} 
          onChange={c => setCrop(c)}
          onDragStart= {e => {}}
          onDragEnd= {e => {
            if(crop?.height == 0 || crop?.width == 0)
              props.onExit();
          }}
          onComplete={handleCompleteCrop}
        >
          <img
            ref={imgRef}
            style={{
              objectFit: "contain",
              height: '100vh',
              width: '100vw',
            }}
            src={props.getPageSrc(props.pageInfo)}
            crossOrigin='anonymous'
            alt="Not loaded"
          />
        </ReactCrop>
      </LocalSpinner>
    </div>
  )
}
