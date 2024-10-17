export interface Environment {
  apiBase: string,
  dashboard: string
  cssVariables: {
    textMain: string;
    bgL1: string;
    bgL2: string;
    bgL3: string;
  },
  jwtToken: string;
}

export interface AlbumDashboardModel {
  fullTitle: string;
  languages: string[];
  isRead: boolean;
  isWip: boolean;
  tier: number;
  pageCount: number;
  note: string;
  coverInfo: FileInfoModel;
}

export interface FileInfoModel {
    name: string;
    extension: string;
    uncPathEncoded: string; //Universal Naming Convention Path

    size: number;
    createDate: Date | string | null;
    updateDate: Date | string | null;
}

export interface TierFractionModel {
    query: string;
    name: string;
    tn: number;
    t0: number;
    t1: number;
    t2: number;
    t3: number;
    ts: number;
}

export interface LogDashboardModel {
    id: string;
    albumFullTitle: string;
    operation: string;
    creationTime: string;

    album?: any;
}

export interface TablePaginationModel<T> {
    totalItem: number;
    totalPage: number;
    records: T[];
}

export interface ForceGraphNode {
    id: string;
    group: number;
    count: number;
}

export interface ForceGraphLink {
    source: string;
    target: string;

    sourceCount: number;
    targetCount: number;
    linkCount: number;

    value: number;
}

export interface ForceGraphData {
    nodes: ForceGraphNode[];
    links: ForceGraphLink[];
}