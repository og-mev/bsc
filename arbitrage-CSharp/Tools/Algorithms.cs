
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace Tools
{
    public class Algorithms
    {
        public List<int> DFS(Graph graph, int start)
        {
            var visited = new List<int>();

            //如果没有这个节点直接返回
            if (!graph.AdjacencyList.ContainsKey(start))
            {
                return visited;
            }
            Stack<int> stack = new Stack<int>();
            //放入起始点
            stack.Push(start);
            while (stack.Count > 0)
            {
                //出栈最后节点
                var vertex = stack.Pop();
                //如果访问过节点 进入下次出栈
                if (visited.Contains(vertex))
                {
                    continue;
                }
                //如果没有访问过这个节点 进栈该节点
                visited.Add(vertex);
                //找最后进栈的节点的所有相邻节点，把其中没有访问过得放入堆栈
                foreach (var neighbor in graph.AdjacencyList[vertex])
                {
                    if (!visited.Contains(neighbor))
                        stack.Push(neighbor);
                }
            }
            return visited;
        }
        /// <summary>
        /// 深度优先 并返回所有路径
        /// 
        /// 如果节点没有重复并且最后一个节点和第一个节点相邻
        /// 要形成环至少要3个节点
        /// </summary>
        /// <typeparam name="int"></typeparam>
        /// <param name="graph"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        public Dictionary<int, List<List<int>>> DFSAllPaths(Graph graph, int start,int maxHops=10)
        {
            var edgeListDic = new Dictionary<string, List<int>>();
            GetAllEdgeList(graph, start, null, edgeListDic);


            var ringEdgeListDic = new Dictionary<string, List<int>>();
            foreach (var item in edgeListDic)
            {
                var v = item.Value;
                if (v.Count>2)
                {


                    foreach (var neighbor in graph.AdjacencyList[v[v.Count-1]])
                    {
                        if (neighbor == start)
                        {
                            v.Add(start);
                            ringEdgeListDic.Add(item.Key, v);
                        }
                    }
                }
            }
            return null;
        }

        public void GetAllEdgeList(Graph graph, int start ,List<int> edgeList = null,  Dictionary<string, List<int>> edgeListDic=null )
        {
            if (edgeList==null)
            {
                edgeList = new List<int>();
                edgeList.Add(start);
            }
            //1 根据当前节点循环相邻节点，把没有的记录edgeListDic
            foreach (var neighbor in graph.AdjacencyList[start])
            {
                string key = string.Join("_", edgeList);
                var strs = key.Split('_');
                int c = 0;
                if (strs.Length>1)
                {
                    c = 1;
                }
                var newK = key.Substring(strs[0].Length+c);

                string reKey = strs[0]+"_" + new string(newK.ToCharArray().Reverse().ToArray())  ;

                if (!edgeListDic.ContainsKey(key) && !edgeListDic.ContainsKey(reKey))
                {
                    edgeListDic.Add(key, edgeList);
                }
                List<int> edgeListClone = new List<int>();
                foreach (var item in edgeList)
                {
                    edgeListClone.Add(item);
                }
                //有重复节点结束，没有重复节点继续寻找
                if (!edgeListClone.Contains(neighbor))
                {
                    edgeListClone.Add(neighbor);

                    GetAllEdgeList(graph, neighbor, edgeListClone, edgeListDic);
                }
                else
                {
                    edgeListClone.Add(neighbor);
                }
                
                
            }
        }
        public static void Test()
        {
            int[] vertices = new[] { 1, 2, 3, 4, 5, 6, 7, 8, 9, 10 };
            Tuple<int, int>[] edges = new[]
            {
            Tuple.Create(1, 2), Tuple.Create(1, 3),
            Tuple.Create(2, 4), Tuple.Create(3, 5), Tuple.Create(3, 6),
            Tuple.Create(4, 7), Tuple.Create(5, 7), Tuple.Create(5, 8),
            Tuple.Create(5, 6), Tuple.Create(8, 9), Tuple.Create(9, 10)
            };

            var graph = new Graph(vertices, edges);
            var algorithms = new Algorithms();

            Logger.Debug(string.Join(", ", algorithms.DFS(graph, 1)));

            algorithms.DFSAllPaths(graph, 1);

            StringBuilder sb = new StringBuilder();
            foreach (var edge in edges)
            {
                sb.AppendLine( $"{edge.Item1} {edge.Item2} 100");
            }

            for (int i = 1; i < vertices.Length+1; i++)
            {
                sb.AppendLine($"{i}");
            }
            Logger.Debug(sb.ToString());
        }
    }
    public class Graph
    {
        public Graph() { }

        public Graph(IEnumerable<int> vertices, IEnumerable<Tuple<int, int>> edges)
        {
            foreach (var vertex in vertices)
            {
                AddVertex(vertex);
            }

            foreach (var edge in edges)
            {
                AddEdge(edge);
            }
        }

        public Dictionary<int, HashSet<int>> AdjacencyList { get; } = new Dictionary<int, HashSet<int>>();
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="vertex"></param>
        private void AddVertex(int vertex)
        {
            AdjacencyList[vertex] = new HashSet<int>();
        }
        /// <summary>
        /// 添加边,添加每个节点的相邻节点
        /// </summary>
        /// <param name="edge"></param>
        private void AddEdge(Tuple<int, int> edge)
        {
            if (AdjacencyList.ContainsKey(edge.Item1) && AdjacencyList.ContainsKey(edge.Item2))
            {
                AdjacencyList[edge.Item1].Add(edge.Item2);
                AdjacencyList[edge.Item2].Add(edge.Item1);
            }
        }
    }


}
