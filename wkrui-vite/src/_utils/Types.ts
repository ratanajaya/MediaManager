export type AlbumInfoVm = {
  povs: string[];
  focuses: string[];
  others: string[];
  rares: string[];
  qualities: string[];
  characters: string[],
  categories: string[],
  orientations: string[]
  languages: string[],
  suitableImageFormats: string[],
  suitableVideoFormats: string[]
}

export type QueryVm = {
  name: string,
  tier: number,
  query: string
}

export type AlbumCardModel = {
  path: string;
  languages: string[];
  isRead: boolean;
  isWip: boolean;
  hasSource: boolean;
  tier: number;
  lastPageIndex: number;
  lastPageAlRelPath: string | null;
  pageCount: number;
  note: string;
  coverInfo: FileInfoModel;
  entryDate: string;
  correctablePageCount: number;

  title: string;
  artistDisplay: string;
}

export type AlbumCardGroup = {
  name: string;
  albumCms: AlbumCardModel[];
}

export type AlbumPageInfo = {
    orientation: string;
    fileInfos: FileInfoModel[];
}

export type FileInfoModel = {
  name: string;
  extension: string;
  libRelPath: string;
  size: number;
  createDate: string | null;
  updateDate: string | null;
  orientation: number | null;
  height: number;
  width: number;
}

export interface HasChildren {
  children: JSX.Element
}

export interface HasLibraryType {
  type: number
}

export type PageDeleteModel = {
  albumPath: string,
  alRelPath: string
}

export type PageMoveModel = {
  overwrite: boolean,
  src: {
    albumPath: string,
    alRelPath: string,
  },
  dst: {
    albumPath: string,
    alRelPath: string
  }
};

export interface MoveFileInfo{
  name: string,
  size: number,
  createdDate: string
}

export type MoveFileResponse = {
  message: string,
  srcInfo: MoveFileInfo | null,
  dstInfo: MoveFileInfo | null
}

export type ChapterVM = {
  title: string;
  pageIndex: number;
  pageUncPath: string;
  tier: number;
  pageCount: number;
}

export type QueryPart = {
  query: string,
  page: number,
  row: number,
  path: string
};

export type Album = {
  title: string;
  category: string;
  orientation: string;

  artists: string[];
  tags: string[];
  povs: string[];
  focuses: string[];
  others: string[];
  rares: string[];
  qualities: string[];

  characters: string[],
  languages: string[];
  sources: Source[];
  note: string;

  tier: number;

  isWip: boolean;
  isRead: boolean;

  entryDate: string | null;
}

export type AlbumVM = {
  path: string;
  pageCount: number;
  lastPageIndex: number;
  coverInfo?: FileInfoModel;
  album: Album;
}

export interface Source {
  title: string | null;
  subTitle: string | null;
  url: string;
}

export interface Comment {
  id: number;
  url: string;

  author: string;
  content: string;
  score: number | null;
  postedDate: string | null;
  
  createdDate: string;
  updatedDate: string | null;
}

export interface SourceAndContent {
  source: Source;
  comments: Comment[];
}

export interface SourceAndContentUpsertModel {
  albumPath: string;
  sourceAndContent: SourceAndContent;
}

export interface SourceDeleteModel {
  albumPath: string;
  url: string;
}

export interface SourceUpdateModel {
  albumPath: string;
  source: Source;
}

export type LogDashboardModel = {
  id: string;
  albumFullTitle: string;
  operation: string;
  creationTime: string;

  album: Album;
}

export interface CompressionCondition {
  width: number;
  height: number;
  quality: number;
}

export interface FileCorrectionModel {
    alRelPath: string;
    extension: string;
    height: number;
    width: number;
    modifiedDate: string;
    byte: number;
    bytesPer100Pixel: number;

    correctionType: number | null;

    upscaleMultiplier: number | null;
    compression: CompressionCondition;
}

export interface FileCorrectionReportModel {
    alRelPath: string;
    success: boolean;
    message: string;

    height: number;
    width: number;

    byte: number;
    bytesPer100Pixel: number;
}

export interface PathCorrectionModel {
  libRelPath: string;
  lastCorrectionDate: string | null;
  correctablePageCount: number;
}

export interface CorrectPageParam {
  type: number;
  libRelPath: string;
  thread: number;
  upscalerType: number;
  fileToCorrectList: FileCorrectionModel[];
  toWebp: boolean;
}

export enum NodeType {
  Folder = 0,
  File = 1
}

export interface FsNode {
  nodeType: NodeType;
  alRelPath: string;

  fileInfo: FileInfoModel | null;
  dirInfo: DirInfoModel | null;
}

export interface DirInfoModel {
  name: string;
  tier: number;
  childs: FsNode[];
}

export interface AlbumFsNodeInfo {
  title: string;
  orientation: 'Portrait' | 'Landscape' | 'Auto';
  fsNodes: FsNode[];
}

export interface KeyValuePair<T1,T2> {
  key: T1;
  value: T2;
}

export interface SettingVal {
  apiBaseUrl: string;
  mediaBaseUrl: string;
  resMediaBaseUrl: string;
  alwaysPortrait: boolean;
  muteVideo: boolean;
  directFileAccess: boolean;
}

export interface EventStreamData {
  isError: boolean,
  maxStep: number,
  currentStep: number,
  message: string
}

export interface OfflineAlbumCm extends AlbumCardModel { 
  fsNodes: FsNode[];
}

export interface OfflineFile {
  id: string;
  albumPath: string;
  alRelPath: string;
  blob: Blob;
}

export interface OcrResult {
  original: string;
  romanized: string;
  english: string;

  error: string;
}

export interface Response {
    success: boolean;
    message: string | null;
}

export interface ResponseResult<T> extends Response {
    result: T;
}

export interface SignalrMessage<T> {
  messageType: SignalrMessageType;
  message: string | null;
  data: T | null;
}

export enum SignalrMessageType {
  Progress = 1,
  Complete = 2,
  Error = 3,
}