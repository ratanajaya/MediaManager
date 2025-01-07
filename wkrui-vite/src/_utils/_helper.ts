import moment from "moment";
import { FileInfoModel, FsNode, NodeType } from "_utils/Types";
import { PagingIndex } from "_shared/ReaderModal/ReaderDisplay";

const _helper = {
  firstLetterLowerCase: (src: string) => {
    return src.charAt(0).toLowerCase() + src.slice(1);
  },

  equalIgnoreCase: (str1: string | null | undefined, str2: string | null | undefined) => {
    return (str1 ?? '').toLowerCase() === (str2 ?? '').toLowerCase();
  },

  getPercent100: (value: number, maxValue: number) => {
    if(maxValue === 0) return 0;
  
    const fraction = (value / maxValue) * 100;
    return fraction <= 100 ? fraction : 100;
  },

  clamp: (src: number, min: number, max: number) => {
    return Math.max(min, Math.min(src, max));
  },

  pathJoin: (parts: string[], sep: string) => {
    const validParts = parts.filter(e => { return e != null && e !== "" });
    const separator = sep || '/';
    const replace   = new RegExp(separator+'{1,}', 'g');
    
    return validParts.join(separator).replace(replace, separator);
  },
  
  formatBytes: (bytes: number, decimals: number = 2) => {
    if (bytes === 0) return '0 Bytes';
  
    const k = 1024;
    const dm = decimals < 0 ? 0 : decimals;
    const sizes = ['Bytes', 'KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'];
  
    const i = Math.floor(Math.log(bytes) / Math.log(k));
  
    return parseFloat((bytes / Math.pow(k, i)).toFixed(dm)) + ' ' + sizes[i];
  },
  formatBytes2: (bytes: number | null | undefined) => {
    if(!bytes)
      return null;
  
    const dp = 1;
    const si = true;
    const thresh = si ? 1000 : 1024;
  
    if (Math.abs(bytes) < thresh) {
      return bytes + ' B';
    }
  
    const units = si 
      ? ['KB', 'MB', 'GB', 'TB', 'PB', 'EB', 'ZB', 'YB'] 
      : ['KiB', 'MiB', 'GiB', 'TiB', 'PiB', 'EiB', 'ZiB', 'YiB'];
    let u = -1;
    const r = 10**dp;
  
    do {
      bytes /= thresh;
      ++u;
    } while (Math.round(Math.abs(bytes) * r) / r >= thresh && u < units.length - 1);
  
    return bytes.toFixed(dp) + ' ' + units[u];
  },
  
  formatDate: (isoDateStr: string) => {
    return new Date(isoDateStr).toLocaleString();
  },
  
  formatNullableDatetime: (dt: string | null) => {
    if(!dt || !moment.utc(dt).isValid()) return '---';
  
    return moment.utc(dt).format('YYYY-MM-DD HH:mm:ss');
  },
  
  formatNullableDatetime2: (dt: string | null) => {
    if(!dt || !moment.utc(dt).isValid()) return '---';
  
    return moment.utc(dt).format('YYYY-MM-DD HH:mm');
  },
  
  ColorFromIndex: (tier: number, e: number) => {
    const top = { h: 180, s: 70, l: 60 };
    const mid = { h: 100, s: 70, l: 60 };
    const bot = { h: 0, s: 70, l: 60 };

    const hsl = e === 3 ? top : e === 2 ? mid : bot;
    const alpha = tier >= e ? 1 : 0;

    return `hsla(${hsl.h},${hsl.s}%,${hsl.l}%,${alpha})`;
  },

  BorderFromIndex: (tier: number, e: number) => {
    return tier > e && (e === 1 || e === 2) ? "2px solid black" : "0px";
  },

  BorderRightFromIndex: (tier: number, e: number) => {
    return tier >= e ? "1px solid black" : "0px";
  },

  getRandomInt: (min: number, max: number) => {
    return Math.floor(Math.random() * (max - min) + min); //The maximum is exclusive and the minimum is inclusive
  },

  isNullOrEmpty: (src: string | null | undefined) => {
    return !src || src === '';
  },

  nz: (src: string | null | undefined, alt: string) => {
    return !_helper.isNullOrEmpty(src) ? src! : alt;
  },

  nzz: (con: string | null | undefined, res1: string, res2: string): string => {
    return !_helper.isNullOrEmpty(con) ? res1 : res2;
  },

  nzAny: <T>(con: T | null | undefined, alt: T): T => {
    return con ? con : alt;
  },

  countFileNodes: (nodes: FsNode[]) => {
    let count = 0;
    nodes.forEach(n => {
      if(n.nodeType === NodeType.File){
        count++;
      }
      else{
        count += _helper.countFileNodes(n.dirInfo!.childs);
      }
    });
  
    return count;
  },
  
  getFlatFileInfosFromFsNodes: (nodes: FsNode[]) => {
    let result: FileInfoModel[] = [];
    nodes.forEach(n => {
      if(n.nodeType === NodeType.File){
        result.push(n.fileInfo!);
      }
      else{
        result = result.concat(_helper.getFlatFileInfosFromFsNodes(n.dirInfo!.childs));
      }
    });
  
    return result;
  },

  getFlatFilesFsNodes: (nodes: FsNode[]) => {
    let result: FsNode[] = [];
    nodes.forEach(n => {
      if(n.nodeType === NodeType.File){
        result.push(n);
      }
      else{
        result = result.concat(_helper.getFlatFilesFsNodes(n.dirInfo!.childs));
      }
    });
  
    return result;
  },
  
  findFileNodeIndex: (nodes: FsNode[], alRelPath: string) => {
    let i : number = 0;
    nodes.forEach(n => {
      if(n.nodeType === NodeType.File){
        if(n.alRelPath === alRelPath)
          return i;
        else
          i++;
      }
      else{
        let j = 0;
        n.dirInfo!.childs.forEach(nc => {
          if(nc.nodeType === NodeType.File){
            if(nc.alRelPath === alRelPath)
              return i + j;
            else
              j++;
          }
        });
      }
    });
  
    return 0;
  },

  limitStringToNCharsWithTrailingPad: (src: string, n: number, pad:string) => {
    if(src.length <= n)
      return src;

    return src.slice(0, n - 1) + pad;
  },

  limitStringToNCharsWithLeadingPad: (src: string, n: number, pad:string) => {
    if(src.length <= n)
      return src;

    return pad + src.slice(src.length - n);
  },

  removeHttpsAndTrailingSlash: (inputString: string) => {
    // Remove leading 'https://'
    let result = inputString.startsWith('https://') ? inputString.slice(8) : inputString;
  
    // Remove trailing '/'
    result = result.endsWith('/') ? result.slice(0, -1) : result;
  
    return result;
  },
  
  isVideo: (extension: string) => {
    return extension === ".webm" || extension === ".mp4";
  },

  getStyleByRotation2: (isRotated: boolean): StyleByRotation => {
    return isRotated ? {
      transformStyle: {
        transform: "translatex(calc(50vw - 50%)) translatey(calc(50vh - 50%)) rotate(270deg)",
      },
      vwvhStyle: {
        width: "100vh",
        height: "100vw",
      }
    } : {
      transformStyle: {},
      vwvhStyle: {
        width: "100vw",
        height: "100vh"
      }
    }
  },

  getIndexes(targetPageIndex: number, pagesLen: number, cPageI?: number): PagingIndex {
    const maxIndex = pagesLen - 1;
    const newPPageI = cPageI ?? 0;
    const newCPageI = _helper.clamp(targetPageIndex, 0, maxIndex);

    const { cSlide, slideAIndex, slideBIndex, slideCIndex } = ((): {
      cSlide: string, slideAIndex: number, slideBIndex: number, slideCIndex: number
    } => {
      function pageCeil(modifier: number) {
        return (((newCPageI + modifier) % pagesLen) + pagesLen) % pagesLen;
      }

      if (newCPageI % 3 === 0) {
        return {
          cSlide: "slideA",
          slideAIndex: newCPageI,
          slideBIndex: pageCeil(1),
          slideCIndex: pageCeil(-1)
        }
      }
      if (newCPageI % 3 === 1) {
        return {
          cSlide: "slideB",
          slideAIndex: pageCeil(-1),
          slideBIndex: newCPageI,
          slideCIndex: pageCeil(1)
        }
      }
      
      return {
        cSlide: "slideC",
        slideAIndex: pageCeil(1),
        slideBIndex: pageCeil(-1),
        slideCIndex: newCPageI
      }
    })();

    const result: PagingIndex = {
      cPageI: newCPageI,
      pPageI: newPPageI,
      cSlide: cSlide,
      slideAIndex: slideAIndex,
      slideBIndex: slideBIndex,
      slideCIndex: slideCIndex
    }

    return result;
  },

  decodeHTMLEntities(text: string | null | undefined) {
    const parser = new DOMParser();
    return parser.parseFromString(`<!doctype html><body>${text}`, 'text/html').body.textContent;
  },
};

export default _helper;

export interface StyleByRotation {
  transformStyle: {
      transform?: string;
  };
  vwvhStyle: {
      width: string;
      height: string;
  };
}