namespace Drive.Graphs;

public interface IAnalyzer<out TAnalysis>
{
	TAnalysis Analyze(Graph graph);
}