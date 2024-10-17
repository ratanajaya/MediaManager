import _constant from "_utils/_constant";
import _helper from "_utils/_helper";
import _ls from "_utils/_ls";
import { SettingVal } from "_utils/Types";

const publicApiUrl = 'https://wkr.azurewebsites.net/';
const publicMediaUrl_ = 'https://wkrstorage.file.core.windows.net/library/[libRelPath]?sv=2020-08-04&ss=f&srt=sco&sp=r&se=2023-04-08T01:17:28Z&st=2022-04-07T17:17:28Z&spr=https&sig=%2BdlHr0HKeGENqIjaCXYA%2B1FNdPV7yKVi9ewlvaAhhG0%3D&antiCache=[antiCache]';

const setting: SettingVal = _ls.load<SettingVal>(_constant.lsKey.setting) ?? _constant.defaultSetting;

const apiBaseUrl = (() => {
  if(_constant.isPublic) return publicApiUrl;

  return setting.apiBaseUrl;
})();

const mediaBaseUrl = (() => {
  if(_constant.isPublic) return publicMediaUrl_;

  return setting.mediaBaseUrl;
})();

const resMediaBaseUrl = (() => {
  if(_constant.isPublic) return publicMediaUrl_;

  return setting.resMediaBaseUrl;
})();

const static_ = `${apiBaseUrl}Static/`;
const main = `${apiBaseUrl}Main/`;
const sc = `${apiBaseUrl}Sc/`;
const dashboard = `${apiBaseUrl}Dashboard/`;
const pc = `${apiBaseUrl}Pc/`;
const extraInfo = `${apiBaseUrl}ExtraInfo/`;
const operation = `${apiBaseUrl}Operation/`;
const sse = `${apiBaseUrl}Sse/`;
const media = mediaBaseUrl;
const resMedia = resMediaBaseUrl;

const GetApiBaseUrl = () => {
  return apiBaseUrl;
}
const SaveApiBaseUrl = (url: string) => {
  window.localStorage.setItem('apiBaseUrl', url);
}

const GetMediaBaseUrl = () => {
  return mediaBaseUrl;
}
const SaveMediaBaseUrl = (url: string) => {
  window.localStorage.setItem('mediaBaseUrl', url);
}

const GetResMediaBaseUrl = () => {
  return resMediaBaseUrl;
}
const SaveResMediaBaseUrl = (url: string) => {
  window.localStorage.setItem('resMediaBaseUrl', url);
}

function withBase(type: number, endpoint: string){
  const base = type === 1 ? sc : main;
  return `${base}${endpoint}`;
}

//#region Static Info
const GetAlbumInfo = () => {
  return `${static_}GetAlbumInfo`;
}
const GetUpscalers = () => {
  return `${static_}GetUpscalers`;
}
//#endregion

//#region Album Query
const GetGenreCardModels = () => {
  return `${main}GetGenreCardModels`;
}
const GetFeaturedArtistCardModels = () => {
  return `${main}GetFeaturedArtistCardModels`;
}
const GetFeaturedCharacterCardModels = () => {
  return `${main}GetFeaturedCharacterCardModels`;
}
const GetAlbumVm = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `GetAlbumVm?path=${cleanPath}`);
}
const GetAlbumCardModels = (type: number, page: number, row: number, query: string) => {
  const cleanQuery = query !== undefined ? encodeURIComponent(query) : "";
  return withBase(type, `GetAlbumCardModels?page=${page}&row=${row}&query=${cleanQuery}`);
}
const GetLibraryDirNodes = (type: number, path?: string, includeChild?: boolean) => {
  const icParam = `?includeChild=${includeChild ? 'true' : 'false'}`;
  const pathParam = _helper.nzz(path, `&path=${path}`, '');
  return withBase(type, `GetLibraryDirNodes${icParam}${pathParam}`);
}
//#endregion

//#region Album Command
const UpdateAlbum = (type?: number) => {
  return `${main}UpdateAlbum`;
}
const UpdateAlbumOuterValue = (type: number) => {
  return withBase(type, 'UpdateAlbumOuterValue');
}
const UpdateAlbumTier = (type: number) => {
  return withBase(type, 'UpdateAlbumTier');
}
const RecountAlbumPages = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `RecountAlbumPages?path=${cleanPath}`);
}
const RefreshAlbum = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `RefreshAlbum?path=${cleanPath}`);
}
const DeleteAlbum = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `DeleteAlbum?path=${cleanPath}`);
}
//#endregion

//#region Page Query
const GetAlbumChapters = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `GetAlbumChapters?path=${cleanPath}`);
}
const GetAlbumPageInfos = (type: number, path: string, includeDetail: boolean, includeDimension: boolean) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `GetAlbumPageInfos?path=${cleanPath}&includeDetail=${includeDetail}&includeDimension=${includeDimension}`);
}
const GetAlbumFsNodes = (type: number, path: string) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `GetAlbumFsNodes?path=${cleanPath}`);
}
const GetAlbumFsNodeInfo = (type: number, path: string, includeDetail: boolean, includeDimension: boolean) => {
  const cleanPath = encodeURIComponent(path);
  return withBase(type, `GetAlbumFsNodeInfo?path=${cleanPath}&includeDetail=${includeDetail}&includeDimension=${includeDimension}`);
}
//#endregion

//#region Page Command
const MoveFile = (type: number) => {
  return withBase(type, `MoveFile`);
}

const DeleteFile = (type: number, path: string, alRelPath: string) => {
  const cleanPath = encodeURIComponent(path);
  const cleanAPath = encodeURIComponent(alRelPath);
  return withBase(type, `DeleteFile?path=${cleanPath}&alRelPath=${cleanAPath}`)
}

const DeleteAlbumChapter = (type: number, path: string, chapterName: string) => {
  const cleanPath = encodeURIComponent(path);
  const cleanCName = encodeURIComponent(chapterName);
  return withBase(type, `DeleteAlbumChapter?path=${cleanPath}&chapterName=${cleanCName}`);
}

const UpdateAlbumChapter = (type: number) => {
  return withBase(type, `UpdateAlbumChapter`);
}
//#endregion

//#region Library Command
const RescanDatabase = () => {
  return `${sse}RescanDatabase`;
}
const ReloadDatabase = () => {
  return `${sse}ReloadDatabase`;
}
//#endregion

//#region Censorship
const Censorship = () => {
  return `${apiBaseUrl}Censorship`;
}
//#endregion

//#region Media
const StreamPage = (libRelPath: string, type?: number) => {
  const cleanType = type ?? 0;
  const cleanLibRelPath = encodeURIComponent(libRelPath);

  return media.replace('[apiBaseUrl]',apiBaseUrl).replace('[libRelPath]',cleanLibRelPath).replace('[type]',cleanType.toString()).replace('[antiCache]', (new Date()).toISOString());
}

const StreamResizedImage = (libRelPath: string, maxSize: number, type?: number) => {
  const cleanType = type ?? 0;
  const cleanLibrelPath = encodeURIComponent(libRelPath);
  
  return resMedia.replace('[apiBaseUrl]',apiBaseUrl).replace('[libRelPath]',cleanLibrelPath).replace('[maxSize]',maxSize.toString()).replace('[type]',cleanType.toString()).replace('[antiCache]', (new Date()).toISOString());
}
//#endregion

//#region Dashboard
const GetDeleteLogs = (query: string, includeAlbum?: boolean) => {
  return `${dashboard}GetDeleteLogs?query=${query}&includeAlbum=${includeAlbum ?? false}`;
}

const GetTagForceGraphData = () => {
  return `${dashboard}GetTagForceGraphData`;
}

const GetGenreTierFractions = () => {
  return `${dashboard}GetGenreTierFractions`;
}

const GetQueryTierFraction = (query: string) => {
  const cleanQuery = encodeURIComponent(query);
  return `${dashboard}GetQueryTierFraction?query=${cleanQuery}`;
}

const GetLogs = (page: number, row: number, operation: string, freeText:string, startDate: string, endDate: string) => {
  const freeTextParam = !_helper.isNullOrEmpty(freeText) ? `&freeText=${encodeURIComponent(freeText)}` : '';
  const operationParam = !_helper.isNullOrEmpty(operation) ? `&operation=${operation}` : '';
  const startDateParam = !_helper.isNullOrEmpty(startDate) ? `&startDate=${startDate}` : '';
  const endDateParam = !_helper.isNullOrEmpty(endDate) ? `&endDate=${endDate}` : '';
  return `${dashboard}GetLogs?page=${page}&row=${row}${operationParam}${freeTextParam}${startDateParam}${endDateParam}`;
}

//#endregion

//#region Pc
const Sleep = () => {
  return `${pc}Sleep`;
}

const Hibernate = () => {
  return `${pc}Hibernate`;
}
//#endregion

//#region ExtraInfo
const GetComments = (url: string) => {
  const cleanUrl = encodeURIComponent(url);
  return `${extraInfo}GetComments?url=${cleanUrl}`;
}

const UpsertSourceAndContent = () => {
  return `${extraInfo}UpsertSourceAndContent`;
}

const DeleteSource = () => {
  return `${extraInfo}DeleteSource`;
}

const UpdateSource = () => {
  return `${extraInfo}UpdateSource`;
}
//#endregion

//#region Operation
const HScanCorrectiblePaths = () => {
  return `${operation}HScanCorrectiblePaths`;
}

const ScGetCorrectablePaths = () => {
  return `${operation}ScGetCorrectablePaths`;
}

const ScFullScanCorrectiblePaths = () => {
  return `${operation}ScFullScanCorrectiblePaths`;
}

const GetCorrectablePages = (type: number, path: string, thread: number, upscaleTarget: number, clampToTarget: boolean) => {
  const cleanPath = encodeURIComponent(path);
  return `${operation}GetCorrectablePages?type=${type}&path=${cleanPath}&thread=${thread}&upscaleTarget=${upscaleTarget}&clampToTarget=${clampToTarget}`;
}

const CorrectPages = () => {
  return `${operation}CorrectPages`;
}

const TranscribeAndTranslateImage = () => {
  return `${operation}TranscribeAndTranslateImage`;
}
//#endregion

//#region Auth
const Login = () => {
  return `${apiBaseUrl}Auth/Login`;
}
//#endregion

const _uri = {
  SaveApiBaseUrl,
  SaveMediaBaseUrl,
  SaveResMediaBaseUrl,
  GetApiBaseUrl,
  GetMediaBaseUrl,
  GetResMediaBaseUrl,

  GetAlbumInfo,
  GetUpscalers,

  GetAlbumVm,
  GetAlbumCardModels,
  GetGenreCardModels,
  GetFeaturedArtistCardModels,
  GetFeaturedCharacterCardModels,
  GetLibraryDirNodes,

  UpdateAlbum,
  UpdateAlbumOuterValue,
  UpdateAlbumTier,
  RecountAlbumPages,
  RefreshAlbum,
  DeleteAlbum,

  GetAlbumChapters,
  GetAlbumPageInfos,
  GetAlbumFsNodes,
  GetAlbumFsNodeInfo,

  MoveFile,
  DeleteFile,
  DeleteAlbumChapter,
  UpdateAlbumChapter,

  RescanDatabase,
  ReloadDatabase,

  Censorship,

  StreamPage, 
  StreamResizedImage,

  GetDeleteLogs,
  GetTagForceGraphData,
  GetGenreTierFractions,
  GetQueryTierFraction,
  GetLogs,

  Sleep,
  Hibernate,

  GetComments,
  UpsertSourceAndContent,
  DeleteSource,
  UpdateSource,

  HScanCorrectiblePaths,
  ScGetCorrectablePaths,
  ScFullScanCorrectiblePaths,
  GetCorrectablePages,
  CorrectPages,
  TranscribeAndTranslateImage,

  Login
};

export default _uri;