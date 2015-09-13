__kernel void
mul(__global const float xDataArray[],
	__global const float yDataArray[],
	__global float rDataArray[])
{
	for(int y=0;y<1080;y++)
		for(int x = 0; x<1920;x++)
			rDataArray[x+y*1920] = xDataArray[x] * yDataArray[y];

};