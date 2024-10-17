import Dexie, { Table } from 'dexie';
import { OfflineAlbumCm, OfflineFile } from '_utils/Types';

export class MySubClassedDexie extends Dexie {
  albumCms!: Table<OfflineAlbumCm>;
  offlineFiles!: Table<OfflineFile>;

  constructor() {
    super('myDatabase');
    this.version(1).stores({
      albumCms: 'path', // Primary key and indexed props
      offlineFiles: 'id, albumPath'
    });
  }
}

const _db = new MySubClassedDexie();

export default _db;