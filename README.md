# PcgRandom

## What is it?

A package with a family of random number generators based on [PCG by M. E. O'Neill][pcg].

It is modeled after [System.Random][msdn], i. e. it subclasses it and therefore tries to give the same semantics.

## Package

I'm using [Semantic Versioning][semver] and will try not to mess things up for the NuGet and [Paket][] consumers.

[pcg]: http://www.pcg-random.org/ "Original implementation"
[paper]: http://www.pcg-random.org/pdf/hmc-cs-2014-0905.pdf "PCG: A Family of Simple Fast Space-Efficient Statistically Good Algorithms for Random Number Generation"
[msdn]: https://msdn.microsoft.com/en-us/library/system.random(v=vs.110).aspx
[semver]: http://semver.org/ "Semantic Versioning (2.0.0)"
[paket]: https://fsprojects.github.io/Paket/ "NuGet made easy"
