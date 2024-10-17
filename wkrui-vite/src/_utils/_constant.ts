import { CSSProperties } from "react";

const imgPlaceholder = 'data:image/png;base64,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';

const colStyle: CSSProperties = {
  textAlign: 'center',
  borderRadius: '8px',
  paddingTop: '4px',
  marginBottom: '4px',
}

const _constant = {
  apiUrl: "https://wkr.azurewebsites.net/", 
  storageUrl: "https://wkrstorage.file.core.windows.net/library/", 
  isPublic: false, 
  albumCardHeight: "150px", 
  colProps: {
    lg: 3,
    md: 6,
    sm: 6,
    xs: 12
  }, 
  colStyle: colStyle,
  orientation: {
    portrait: "Portrait",
    landscape: "Landscape",
    auto: "Auto"
  },
  threadOptions: [
    {value: 1, label: '1'},
    {value: 2, label: '2'},
    {value: 3, label: '3'},
    {value: 4, label: '4'},
    {value: 5, label: '5'},
  ],
  resOptions: [
    {value: 1280, label: '1280px'},
    {value: 1600, label: '1600px'},
    {value: 1920, label: '1920px'},
  ],
  lsKey: {
    selectedTiers: 'selectedTiers',
    setting: 'setting',
    authToken: 'authToken'
  },
  defaultSetting: {
    apiBaseUrl: 'http://localhost:5000/',
    mediaBaseUrl: '[apiBaseUrl]Media/StreamPage?libRelPath=[libRelPath]&type=[type]',
    resMediaBaseUrl: '[apiBaseUrl]Media/StreamResizedImage?libRelPath=[libRelPath]&maxSize=[maxSize]&type=[type]',
    alwaysPortrait: false,
    muteVideo: false,
    directFileAccess: false
  },
  defaultIndexes: {
    cPageI: 0,
    pPageI: 0,
    cSlide: "slideA",
    slideAIndex: 0,
    slideBIndex: 0,
    slideCIndex: 0,
  },
  imgPlaceholder: imgPlaceholder,
}

export default _constant;