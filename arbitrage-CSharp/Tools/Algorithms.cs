
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Tools;

namespace Tools
{
    public class Algorithms
    {
        public static List<T> DFS<T>(Graph<T> graph, T start)
        {
            var visited = new List<T>();

            //如果没有这个节点直接返回
            if (!graph.AdjacencyList.ContainsKey(start))
            {
                return visited;
            }
            Stack<T> stack = new Stack<T>();
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
        public static Dictionary<int, List<List<T>>> DFSAllPaths<T>(Graph<T> graph, T start,int maxHops=10)
        {
            Dictionary<int, List<List<T>>> allRings = new Dictionary<int, List<List<T>>>();
            var edgeListDic = new Dictionary<string, List<T>>();
            //找到所有的 长边
            GetAllEdgeList(graph, start, maxHops, null, edgeListDic);

            //过滤其中成环的和长度过载的
            var ringEdgeListDic = new Dictionary<string, List<T>>();
            foreach (var item in edgeListDic)
            {
                var v = item.Value;
                //成环要求，至少要3个节点。。
                //排除大于最大长度的环
                if (v.Count>2 && v.Count< maxHops)
                {
                    
                    foreach (var neighbor in graph.AdjacencyList[v[v.Count-1]])
                    {
                        if (neighbor.Equals( start))
                        {
                            v.Add(start);
                            ringEdgeListDic.Add(item.Key, v);
                            if (!allRings.TryGetValue(v.Count, out List<List<T>> paths))
                            {
                                paths = new List<List<T>>();
                                allRings[v.Count] = paths;
                            }
                            paths.Add(v);
                        }
                    }
                }
            }
            return allRings;
        }

        public static void GetAllEdgeList<T>(Graph<T> graph, T start, int maxHops,List<T> edgeList = null,  Dictionary<string, List<T>> edgeListDic=null )
        {
            if (edgeList==null)
            {
                edgeList = new List<T>();
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

                string reKey = strs[0]+"_" + new string(newK.ToCharArray().Reverse().ToArray());

                if (!edgeListDic.ContainsKey(key) && !edgeListDic.ContainsKey(reKey))
                {
                    edgeListDic.Add(key, edgeList);
                }
                List<T> edgeListClone = new List<T>();
                foreach (var item in edgeList)
                {
                    edgeListClone.Add(item);
                }
                //有重复节点结束，没有重复节点继续寻找,并且小于最大数量
                if (!edgeListClone.Contains(neighbor) && edgeListClone.Count < maxHops)
                {
                    edgeListClone.Add(neighbor);
                    GetAllEdgeList(graph, neighbor, maxHops, edgeListClone, edgeListDic);
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

            var graph = new Graph<int>(vertices, edges);

            Logger.Debug(string.Join(", ", Algorithms.DFS(graph, 1)));

            Algorithms.DFSAllPaths(graph, 1);

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
    public class Graph<T>
    {
        public Graph() { }

        public Graph(IEnumerable<T> vertices, IEnumerable<Tuple<T, T>> edges)
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
        /// <summary>
        /// 所有节点对应相邻的 节点集合
        /// Dictionary<T 某个节点 , HashSet<T> 这个节点相邻的节点>
        /// </summary>
        public Dictionary<T, HashSet<T>> AdjacencyList { get; } = new Dictionary<T, HashSet<T>>();

        public void AddVertex(T vertex)
        {
            AdjacencyList[vertex] = new HashSet<T>();
        }

        public void AddEdge(Tuple<T, T> edge)
        {
            if (AdjacencyList.ContainsKey(edge.Item1) && AdjacencyList.ContainsKey(edge.Item2))
            {
                AdjacencyList[edge.Item1].Add(edge.Item2);
                AdjacencyList[edge.Item2].Add(edge.Item1);
            }
        }
    }


}
