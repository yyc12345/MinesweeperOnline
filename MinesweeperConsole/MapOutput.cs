using System;
using System.Collections.Generic;
using System.Text;
using MinesweeperLib;

namespace MinesweeperConsole {

    public class MapOutput {

        public MapOutput() {

        }

        #region get character

        private char GetBorder(bool isHorizen, bool isBold = false) {
            if (isHorizen) {
                if (isBold) return (char)9473;
                else return (char)9472;
            } else {
                if (isBold) return (char)9475;
                else return (char)9474;
            }
        }

        private char GetCorner(CornerType type, bool isBold = false) {
            switch (type) {
                case CornerType.TopLeft:
                    if (isBold) return (char)9487;
                    else return (char)9484;
                case CornerType.TopRight:
                    if (isBold) return (char)9491;
                    else return (char)9488;
                case CornerType.BottomLeft:
                    if (isBold) return (char)9495;
                    else return (char)9492;
                case CornerType.BottomRight:
                    if (isBold) return (char)9499;
                    else return (char)9496;
                default:
                    if (isBold) return (char)9547;
                    else return (char)9532;
            }
        }

        private char GetTotalBlock() {
            return (char)9608;
        }

        private enum CornerType {
            TopLeft,
            TopRight,
            BottomLeft,
            BottomRight
        }

        #endregion

        public void Output(Cell[,] cells, Point central) {
            StringBuilder sb = new StringBuilder();
            sb.Append(GetCorner(CornerType.TopLeft));
            sb.Append(GetBorder(true));
            sb.Append(GetBorder(true));
            sb.Append(GetBorder(true));
            sb.Append(GetCorner(CornerType.TopRight));
            string head = sb.ToString();

            sb.Clear();
            sb.Append(GetCorner(CornerType.BottomLeft));
            sb.Append(GetBorder(true));
            sb.Append(GetBorder(true));
            sb.Append(GetBorder(true));
            sb.Append(GetCorner(CornerType.BottomRight));
            string foot = sb.ToString();

            for (int i = 0; i < cells.GetLength(1); i++) {

                for (int q = 0; q < cells.GetLength(0); q++) {

                    if (cells[q, i].Status == CellUserStatus.Unopen) {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }

                    if (q == central.X && i == central.Y) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(head);
                        Console.ForegroundColor = ConsoleColor.White;
                    } else {
                        Console.Write(head);
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write("\n");


                for (int j = 0; j < cells.GetLength(0); j++) {

                    //set bk
                    if (cells[j, i].Status == CellUserStatus.Unopen) {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }

                    //draw border
                    if (j == central.X && i == central.Y) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(GetBorder(false));
                        Console.ForegroundColor = ConsoleColor.White;
                    } else {
                        Console.Write(GetBorder(false));
                    }

                    //judge color
                    if (cells[j, i].IsWrong) {
                        Console.ForegroundColor = ConsoleColor.Red;
                        if (cells[j, i].Status == CellUserStatus.Flag)
                            Console.ForegroundColor = ConsoleColor.Magenta;
                    } else {
                        if (cells[j, i].Status == CellUserStatus.Flag)
                            Console.ForegroundColor = ConsoleColor.Yellow;
                    }

                    //write char
                    Console.Write(" ");
                    switch (cells[j, i].Status) {
                        case CellUserStatus.Blank:
                        case CellUserStatus.Unopen:
                            Console.Write(" ");
                            break;
                        case CellUserStatus.NoLoaded:
                            Console.Write("N");
                            break;
                        case CellUserStatus.Number1:
                            Console.Write("1");
                            break;
                        case CellUserStatus.Number2:
                            Console.Write("2");
                            break;
                        case CellUserStatus.Number3:
                            Console.Write("3");
                            break;
                        case CellUserStatus.Number4:
                            Console.Write("4");
                            break;
                        case CellUserStatus.Number5:
                            Console.Write("5");
                            break;
                        case CellUserStatus.Number6:
                            Console.Write("6");
                            break;
                        case CellUserStatus.Number7:
                            Console.Write("7");
                            break;
                        case CellUserStatus.Number8:
                            Console.Write("8");
                            break;
                        case CellUserStatus.Flag:
                            Console.Write("P");
                            break;
                    }

                    Console.Write(" ");

                    //draw border
                    Console.ForegroundColor = ConsoleColor.White;
                    if (j == central.X && i == central.Y) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(GetBorder(false));
                        Console.ForegroundColor = ConsoleColor.White;
                    } else {
                        Console.Write(GetBorder(false));
                    }

                    //restore bk
                    Console.BackgroundColor = ConsoleColor.Black;

                }
                Console.Write("\n");


                for (int q = 0; q < cells.GetLength(0); q++) {

                    if (cells[q, i].Status == CellUserStatus.Unopen) {
                        Console.BackgroundColor = ConsoleColor.DarkGray;
                    }

                    if (q == central.X && i == central.Y) {
                        Console.ForegroundColor = ConsoleColor.Yellow;
                        Console.Write(foot);
                        Console.ForegroundColor = ConsoleColor.White;
                    } else {
                        Console.Write(foot);
                    }

                    Console.BackgroundColor = ConsoleColor.Black;
                }
                Console.Write("\n");

            }
        }

    }
}
