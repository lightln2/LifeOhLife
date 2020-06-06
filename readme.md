# Conway's Game of Life Performance Optimization

- 100x performance improvement for C# simulation
- 10x performance improvement for JavaScript simulation
- Play <a href="https://lightln2.github.io/LifeOhLife/html/index.html">Conway's Game of Life in Full HD online simulator</a>!

The code shows some optimization techniques that can be used to improve performance significantly:
- Using 64-bit words instead of bytes to reduce number of instructons
- Using bit manipulation tricks to replace conditional branching
- Using SIMD instructions to process up to 32 bytes at once

## Note
This code is optimization of a standard Life simulation algorithm.
There is <a href="https://en.wikipedia.org/wiki/Hashlife">Hashlife</a> algorithm which is much faster on 
most patterns.
Also, GPU optimization is not covered, however running the simulation on GPU might be another order of magnitude faster.

## License

- **[MIT license](https://lightln2.github.io/LifeOhLife/license.txt)**
- Copyright 2020 © lightln2
