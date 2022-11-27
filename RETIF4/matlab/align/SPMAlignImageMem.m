function [reslicedData, rparams] = SPMAlignImageMem(memAlignObject, file)
	
	% test data
	%{
	referenceFilePath = 'D:\\G4\\realignment_volume.img';
	refP = spm_vol(referenceFilePath);
	memAlignObject = RT_spm_realign_mem_createAlignObject(refP);
	file = 'D:\\G5\\cr_Timmy(1).img';
	%}
	
	% load (does not load data, just info)
    P = spm_vol(file);

	% realign
	[P, rparams] = RT_spm_realign_mem(memAlignObject, P);
	
	% reslice
	P = [refP, P];
	[reslicedData, reslicedMat] = RT_spm_reslice(P);
	
	%{
	% write
	VO         = P(2);
	VO.fname   = spm_file(P(2).fname, 'prefix', 'r');
	VO.dim     = P(2).dim(1:3);
	VO.dt      = P(2).dt;
	VO.pinfo   = P(2).pinfo;
	VO.mat     = reslicedMat;
	VO.descrip = 'realigned resliced';
	VO = spm_write_vol(VO, reslicedData);
	%}
	
end
