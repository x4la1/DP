using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared;

public record SimilarityCalculatedEvent(string Id, double Similarity);
public record RankCalculatedEvent(string Id, double Rank);