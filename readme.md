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

## Results
Performance comparison of all algorithms (might be different on your computer):
```
SimpleLife: 22.066 steps/second (46 M cells/sec)

LifeInList: 45.286 steps/second (94 M cells/sec)
LifeIsChange: 77.723 steps/second (161 M cells/sec)

LifeBytes: 44.212 steps/second (92 M cells/sec)
LongLife: 95.506 steps/second (198 M cells/sec)
LifeIsLookingUp: 182.320 steps/second (378 M cells/sec)
LifeIsABitMagic: 1189.921 steps/second (2467 M cells/sec)
LifeInLine_Bytes: 81.620 steps/second (169 M cells/sec)
LifeInLine_Long: 807.081 steps/second (1674 M cells/sec)
LifeInLine_LongCompressed: 1302.980 steps/second (2702 M cells/sec)

AdvancedLifeExtensions: 4604.143 steps/second (9547 M cells/sec)
AdvancedLifeExtensionsInLine: 5182.940 steps/second (10747 M cells/sec)

AdvancedLifeExtensionsInLineCompressed: 7408.325 steps/second (15362 M cells/sec)
```

## License

- **[MIT license](https://lightln2.github.io/LifeOhLife/license.txt)**
- Copyright 2020 © lightln2
