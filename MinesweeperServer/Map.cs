using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Threading;
using MinesweeperLib;

namespace MinesweeperServer {

    public class Map {

        public Map(FilePathBuilder folder, double difficulty, int chunkLength) {
            gameFolder = folder;
            mapDifficulty = difficulty;
            mapChunkLength = chunkLength;

            if (checkInfo() == false) throw new ArgumentException();

            userPos = new Point(0, 0);
        }

        public Map(FilePathBuilder recordFolder) {
            gameFolder = recordFolder;
            LoadMapInfo();
            if (checkInfo() == false) throw new ArgumentException();
        }

        #region general operation

        bool withoutInitialization = true;

        public void Initialize() {
            withoutInitialization = false;

            chunkList = new Dictionary<Point, MapChunk>();
            lockChunkList = new object();

            WeakLoadChunkCleaner();
            RefreshStrongChunk();
            OnRefresh();
        }

        public void Close() {
            FlushAll();
            SaveMapInfo();
        }

        public void Press() {
            Task.Run(() => {
                this.PressCell(this.userPos);

                OnRefresh();
            });
        }

        public Cell[,] GetCellData(Point startPoint, int width, int height) {
            return this.GetCellsRectangle(startPoint, width, height);
        }

        public void Flag() {
            Task.Run(() => {
                var cache = this.GetCell(this.userPos);

                if (cache.Status == CellUserStatus.Flag && cache.IsWrong == false) {
                    this.GetCell(this.userPos).Status = CellUserStatus.Unopen;
                } else if (cache.Status == CellUserStatus.Unopen) {
                    this.GetCell(this.userPos).Status = CellUserStatus.Flag;
                }

                OnRefresh();
            });
        }

        public event Action Refresh;
        public void OnRefresh() {
            Refresh?.Invoke();
        }

        #endregion

        #region suggestions

        public static readonly double DifficulyBeginner = 0.0625;
        public static readonly double DifficulyAmateur = 0.125;
        public static readonly double DifficulyExpert = 0.213;
        public static readonly double DifficulyHell = 0.236;

        public static readonly int MapChunckLengthDefault = 100;
        #endregion

        #region map info

        double mapDifficulty;
        int mapChunkLength;

        FilePathBuilder gameFolder;

        bool checkInfo() {
            if (mapDifficulty < DifficulyBeginner || mapDifficulty > 1) return false;
            if (mapChunkLength < 3) return false;
            return true;
        }

        void SaveMapInfo() {
            //get file
            var path = gameFolder.Clone();
            path.Enter("minesweeper.dat");
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            var file = new BinaryWriter(cache, Encoding.UTF8, true);
            //write data
            file.Write(mapDifficulty);
            file.Write(mapChunkLength);
            file.Write(userPos.ToString());

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        void LoadMapInfo() {
            //get file
            var path = gameFolder.Clone();
            path.Enter("minesweeper.dat");
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.Open, FileAccess.Read, FileShare.None);
            var file = new BinaryReader(cache, Encoding.UTF8, true);
            //read data
            mapDifficulty = file.ReadDouble();
            mapChunkLength = file.ReadInt32();

            userPos = new Point(file.ReadString());

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        #endregion

        #region user info

        public Point UserPos {
            get { return userPos; }
            set {
                userPos = value;
                userChunk = GetChunk(userPos);

                //judge
                if (userChunk != previousUserChunk) {
                    //raise update
                    RefreshStrongChunk();

                    previousUserChunk = userChunk;
                }

                if (!withoutInitialization)
                    OnRefresh();
            }
        }
        Point userPos;
        public Point UserChunk { get { return userChunk; } }
        Point userChunk;
        Point previousUserChunk;

        #endregion

        #region map

        public readonly Cell NoLoadedCell = new Cell() { Status = CellUserStatus.NoLoaded };

        void PressCell(Point pos) {
            lock (lockChunkList) {
                var nineSudoku = GetCellsRectangle(pos - new Point(1, 1), 3, 3);

                //check mine
                if (nineSudoku[1, 1].IsMine) {
                    if (nineSudoku[1, 1].Status == CellUserStatus.Flag) {
                        //mine with flag
                        return;
                    } else {
                        //mine without flag
                        nineSudoku[1, 1].IsWrong = true;
                        nineSudoku[1, 1].Status = CellUserStatus.Flag;
                        return;
                    }
                } else {
                    if (nineSudoku[1, 1].Status == CellUserStatus.Flag) {
                        //flag without mine
                        nineSudoku[1, 1].IsWrong = true;
                    }

                    //calc number
                    int number = 0;
                    int flags = 0;
                    int unopen = 0;
                    for (int i = 0; i < 3; i++) {
                        for (int j = 0; j < 3; j++) {
                            if (!(j == 1 && i == 1)) {
                                if (nineSudoku[i, j].IsMine == true) number++;
                                if (nineSudoku[i, j].Status == CellUserStatus.Flag) flags++;
                                if (nineSudoku[i, j].Status == CellUserStatus.Unopen || nineSudoku[i, j].Status == CellUserStatus.Flag) unopen++;
                            }
                        }
                    }
                    switch (number) {
                        case 0:
                            nineSudoku[1, 1].Status = CellUserStatus.Blank;
                            break;
                        case 1:
                            nineSudoku[1, 1].Status = CellUserStatus.Number1;
                            break;
                        case 2:
                            nineSudoku[1, 1].Status = CellUserStatus.Number2;
                            break;
                        case 3:
                            nineSudoku[1, 1].Status = CellUserStatus.Number3;
                            break;
                        case 4:
                            nineSudoku[1, 1].Status = CellUserStatus.Number4;
                            break;
                        case 5:
                            nineSudoku[1, 1].Status = CellUserStatus.Number5;
                            break;
                        case 6:
                            nineSudoku[1, 1].Status = CellUserStatus.Number6;
                            break;
                        case 7:
                            nineSudoku[1, 1].Status = CellUserStatus.Number7;
                            break;
                        case 8:
                            nineSudoku[1, 1].Status = CellUserStatus.Number8;
                            break;
                    }

                    if (number == unopen) {
                        //check automatical flag
                        //set all unopen cell to be flag
                        for (int i = 0; i < 3; i++) {
                            for (int j = 0; j < 3; j++) {
                                if (!(j == 1 && i == 1)) {
                                    //because the cell, whose status is flag, don't need any operation. so i ignore them.
                                    if (nineSudoku[i, j].Status == CellUserStatus.Unopen) nineSudoku[i, j].Status = CellUserStatus.Flag;
                                }
                            }
                        }

                        return;
                    } else if (flags != number) {
                        //check automatical open
                        //some mine is not flag. pass.
                        return;
                    } else {
                        //automatical open can be done
                        //recursion each cell in nine sudoku
                        Point offset = new Point(0, 0);
                        for (int i = 0; i < 3; i++) {
                            offset.X = i - 1;
                            for (int j = 0; j < 3; j++) {
                                offset.Y = j - 1;
                                //only operate unopen cell
                                if ((!(j == 1 && i == 1)) &&
                                    (nineSudoku[i, j].Status == CellUserStatus.Unopen || nineSudoku[i, j].Status == CellUserStatus.Flag)) {
                                    PressCell(pos + offset);
                                }
                            }
                        }
                    }
                }
            }

            return;
        }

        Cell[,] GetCellsRectangle(Point startPoint, int width, int height) {
            Cell[,] result = new Cell[width, height];

            if (GetChunk(startPoint) == GetChunk(startPoint + new Point(width - 1, height - 1))) {
                //within 1 chunk
                MapChunk chunk;
                if (!chunkList.TryGetValue(GetChunk(startPoint), out chunk)) {
                    //no loaded. weak load now
                    WeakLoadChunk(GetChunk(startPoint));
                    //get chunk
                    chunk = chunkList[GetChunk(startPoint)];
                }

                Point offset = new Point(0, 0);

                for (int i = 0; i < width; i++) {
                    offset.X = i;
                    for (int j = 0; j < height; j++) {
                        offset.Y = j;

                        result[i, j] = chunk[GetChunkInnerPos(startPoint + offset)];
                    }
                }

            } else {
                //multi-chunk read
                Dictionary<Point, MapChunk> cache = new Dictionary<Point, MapChunk>();

                Point offset = new Point(0, 0);
                Point chunk;

                for (int i = 0; i < width; i++) {
                    offset.X = i;
                    for (int j = 0; j < height; j++) {
                        offset.Y = j;

                        chunk = GetChunk(startPoint + offset);
                        if (cache.ContainsKey(chunk))
                            //in cache
                            result[i, j] = cache[chunk][GetChunkInnerPos(startPoint + offset)];
                        else if (chunkList.ContainsKey(chunk)) {
                            //in chunk list
                            cache.Add(chunk, chunkList[chunk]);
                            result[i, j] = cache[chunk][GetChunkInnerPos(startPoint + offset)];
                        } else {
                            //no loaded. weak load now
                            WeakLoadChunk(chunk);
                            //get chunk
                            cache.Add(chunk, chunkList[chunk]);
                            result[i, j] = cache[chunk][GetChunkInnerPos(startPoint + offset)];
                        }
                    }
                }

            }

            return result;
        }

        Cell GetCell(Point pos) {
            try {
                lock (lockChunkList) {
                    return chunkList[GetChunk(pos)][GetChunkInnerPos(pos)];
                }
            } catch (Exception) {
                //no loaded. weak load now
                WeakLoadChunk(GetChunk(pos));
                return chunkList[GetChunk(pos)][GetChunkInnerPos(pos)];
            }
        }

        //void SetCell(Point pos, Cell newCell) {
        //    var innerPos = GetChunkInnerPos(pos);

        //    try {
        //        chunkList[GetChunk(pos)][innerPos] = newCell;
        //    } catch (Exception) {
        //        //fail to search
        //        //pass
        //    }
        //}

        #endregion

        #region map chunk

        Dictionary<Point, MapChunk> chunkList;
        object lockChunkList;

        void WeakLoadChunkCleaner() {
            Task.Run(() => {
                while (true) {
                    //awake regularly (20min)
                    Thread.Sleep(1000 * 60 * 20);
                    if (chunkList.Count >= 100) {
                        FlushWeakLoad();
                    }
                }
            });
        }

        void FlushWeakLoad() {
            lock (lockChunkList) {
                foreach (var item in chunkList.Values) {
                    if (item.IsStrongLoading == false) {
                        //save
                        SaveChunk(item);
                        chunkList.Remove(item.ChunkPosition);
                    }
                }
            }
        }

        void RefreshStrongChunk() {
            lock (lockChunkList) {

                HashSet<string> previousChunk = new HashSet<string>();
                HashSet<string> nowChunk = new HashSet<string>();

                //record pre
                foreach (var item in chunkList.Values) {
                    //only record strong load chunk
                    if (item.IsStrongLoading == true) previousChunk.Add(item.ChunkPosition.ToString());
                }

                //record now
                for (int i = -1; i <= 1; i++) {
                    for (int j = -1; j <= 1; j++) {
                        nowChunk.Add(new Point(this.userChunk.X + i, this.userChunk.Y + j).ToString());
                    }
                }

                //get info
                var deletedChunk = from item in previousChunk
                                   where !nowChunk.Contains(item)
                                   select item;

                var addedChunk = (from item in nowChunk
                                  where !previousChunk.Contains(item)
                                  select item).ToList();

                //process
                //delete
                foreach (var item in deletedChunk) {
                    SaveChunk(chunkList[new Point(item)]);
                    chunkList.Remove(new Point(item));
                }
                //restore weak load
                //and strong load new chunk
                foreach (var item in addedChunk) {
                    if (chunkList.ContainsKey(new Point(item))) chunkList[new Point(item)].IsStrongLoading = true;
                    else StrongLoadChunk(new Point(item));
                }
            }

        }

        void FlushAll() {
            lock (lockChunkList) {
                foreach (var item in chunkList.Values) {
                    SaveChunk(item);
                }
                chunkList.Clear();
            }
        }


        #endregion

        #region map chunk func

        Point GetChunk(Point p) {
            return new Point((p.X >= 0 ? p.X / mapChunkLength : ((p.X + 1) / mapChunkLength) - 1),
                            (p.Y >= 0 ? p.Y / mapChunkLength : ((p.Y + 1) / mapChunkLength) - 1));
        }

        Point GetChunkOriginPoint(Point chunk) {
            return new Point(chunk.X * this.mapChunkLength, chunk.Y * this.mapChunkLength);
        }

        Point GetChunkInnerPos(Point globalPos) {
            return globalPos - GetChunkOriginPoint(GetChunk(globalPos));
        }

        #endregion

        #region map generate func

        Cell[,] GenerateMap() {
            Cell[,] map = new Cell[this.mapChunkLength, this.mapChunkLength];
            var rnd = new Random();

            for (int i = 0; i < this.mapChunkLength; i++) {
                for (int j = 0; j < this.mapChunkLength; j++) {
                    map[i, j] = new Cell();
                    map[i, j].Status = CellUserStatus.Unopen;
                    map[i, j].IsMine = (rnd.NextDouble() < this.mapDifficulty);
                    map[i, j].IsWrong = false;
                }
            }

            return map;
        }

        #endregion

        #region map read func

        /// <summary>
        /// This number indicates how many square chunk groups a file can hold
        /// </summary>
        static readonly int fileChunkLength = 6;
        static readonly byte fileGeneratedSign = 61;

        long GetSingleCellLengthInFile() {
            //a chunk's capacity:
            //1 cell 2 byte
            //multiply the number of cell
            //add 1 byte to sign the generate. this byte is the head of all chunk. if it is 61, this chunk is generated
            return 2 * mapChunkLength * mapChunkLength + 1;
        }

        Point GetFile(Point chunk) {
            return new Point((chunk.X >= 0 ? chunk.X / fileChunkLength : ((chunk.X + 1) / fileChunkLength) - 1),
                            (chunk.Y >= 0 ? chunk.Y / fileChunkLength : ((chunk.Y + 1) / fileChunkLength) - 1));
        }

        string GetFileName(Point file) {
            return file.ToString() + ".msd";

        }

        Point GetFileOriginChunk(Point file) {
            return new Point(file.X * fileChunkLength, file.Y * fileChunkLength);
        }

        long GetFileSeekPos(Point chunkPos) {
            var cache = chunkPos - GetFileOriginChunk(GetFile(chunkPos));
            return long.Parse((cache.Y * fileChunkLength * GetSingleCellLengthInFile() + cache.X * GetSingleCellLengthInFile()).ToString());
        }

        Cell[,] LoadChunk(Point chunk) {
            //get file
            Point filePos = GetFile(chunk);
            var path = gameFolder.Clone();
            path.Enter(GetFileName(filePos));
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.Read, FileShare.None);
            cache.Seek(GetFileSeekPos(chunk), SeekOrigin.Begin);
            var file = new BinaryReader(cache, Encoding.UTF8, true);
            //get data
            Cell[,] map;

            //if this area's map is not generated. generate it now and load it wait to save
            try {
                if (file.ReadByte() != fileGeneratedSign) goto generate;
            } catch (Exception) {
                goto generate;
            }

            map = new Cell[this.mapChunkLength, this.mapChunkLength];
            for (int i = 0; i < this.mapChunkLength; i++) {
                for (int j = 0; j < this.mapChunkLength; j++) {
                    map[i, j] = new Cell();
                    map[i, j].Status = (CellUserStatus)file.ReadByte();
                    map[i, j].IsMine = file.ReadByte() == 1;
                    map[i, j].IsWrong = file.ReadByte() == 1;
                }
            }
            goto close;

        generate:
            map = GenerateMap();
        close:
            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();

            return map;
        }

        void SaveChunk(MapChunk chunk) {
            //get file
            Point filePos = GetFile(chunk.ChunkPosition);
            var path = gameFolder.Clone();
            path.Enter(GetFileName(filePos));
            //open and seek file
            var cache = new FileStream(path.Path, FileMode.OpenOrCreate, FileAccess.ReadWrite, FileShare.None);
            cache.Seek(GetFileSeekPos(chunk.ChunkPosition), SeekOrigin.Begin);
            var file = new BinaryWriter(cache, Encoding.UTF8, true);
            //write data
            Point offset = new Point(0, 0);
            file.Write((byte)fileGeneratedSign);
            for (int i = 0; i < this.mapChunkLength; i++) {
                offset.X = i;
                for (int j = 0; j < this.mapChunkLength; j++) {
                    offset.Y = j;
                    file.Write((byte)(chunk[offset].Status));
                    file.Write((byte)(chunk[offset].IsMine ? 1 : 0));
                    file.Write((byte)(chunk[offset].IsWrong ? 1 : 0));
                }
            }

            //close
            file.Close();
            file.Dispose();
            cache.Close();
            cache.Dispose();
        }

        void StrongLoadChunk(Point chunk) {
            this.chunkList.TryAdd(chunk, new MapChunk(chunk, LoadChunk(chunk), this.mapChunkLength, true));
        }

        void WeakLoadChunk(Point chunk) {
            this.chunkList.TryAdd(chunk, new MapChunk(chunk, LoadChunk(chunk), this.mapChunkLength, false));
        }

        #endregion

    }


}
