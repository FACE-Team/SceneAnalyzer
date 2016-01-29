// VisualSaliency.h
//#pragma once

// The following ifdef block is the standard way of creating macros which make exporting 
// from a DLL simpler. All files within this DLL are compiled with the VISUALSALIENCY_EXPORTS
// symbol defined on the command line. this symbol should not be defined on any project
// that uses this DLL. This way any other project whose source files include this file see 
// VISUALSALIENCY_API functions as being imported from a DLL, whereas this DLL sees symbols
// defined with this macro as being exported.
#ifdef VISUALSALIENCY_EXPORTS
#define VISUALSALIENCY_API __declspec(dllexport)
#else
#define VISUALSALIENCY_API __declspec(dllimport)
#endif

#include <Windows.h>
#include "../include/opencv2/imgproc/imgproc.hpp"
#include "../include/opencv2/opencv.hpp"
#include "FastSalience.h"
#include <vector>
#include <string>
#include <iostream>
#include <cstdio>

using namespace std;
using namespace cv;

extern "C" {
	struct SalientPoint
	{
	   int x;
	   int y;
	   int frameWidth;
	   int frameHeight;
	};

	typedef struct _VisualSaliency VisualSaliency;

	VISUALSALIENCY_API int WINAPI InitCamera(CvCapture* capt);
	VISUALSALIENCY_API int WINAPI ShowCamera();
	VISUALSALIENCY_API void WINAPI StopCamera();

	VISUALSALIENCY_API VisualSaliency* WINAPI CreateVisualSaliency(int width, int height, int type, int numtemporal, int numspatial, float firsttau, int firstrad);
	VISUALSALIENCY_API void WINAPI DestroyVisualSaliency(VisualSaliency* vs);
	VISUALSALIENCY_API void WINAPI UpdateVisualSaliency(VisualSaliency* vs, void* data);
	VISUALSALIENCY_API SalientPoint WINAPI GetSalientPoint(VisualSaliency* vs);
	VISUALSALIENCY_API void* WINAPI GetSaliencyMap(VisualSaliency* vs);
	VISUALSALIENCY_API void* WINAPI GetSaliencyMapRgb(VisualSaliency* vs);
	VISUALSALIENCY_API int WINAPI GetSaliencyMapType(VisualSaliency* vs);

	//VISUALSALIENCY_API void WINAPI CalculateSaliencyMap(void* data,int row, int col, int color, int numtemporal, int numspatial, float firsttau, int firstrad);
	VISUALSALIENCY_API void WINAPI SaliencyMap( struct SalientPoint *sPoint, int numtemporal, int numspatial, float firsttau, int firstrad, int wFrameResized);

}
