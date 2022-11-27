function [reslicedData, rparams] = SPMAlignImageMem2(memAlignObject, vol, volData)
	
	% test data
	%{
	refP = spm_vol('D:\\G4\\realignment_volume.img');
	memAlignObject = RT_spm_realign_mem_createAlignObject(refP);
	vol = spm_vol('D:\\G5\\cr_Timmy(1).img');
	volData = spm_read_vols(vol);
	%}
	
	% realign (estimate)
	[outVolMat, rparams] = RT_spm_realign_mem2(memAlignObject, vol.mat, volData);
	vol.mat = outVolMat;	% transfer the realigned matrix
	
	% reslice
	[reslicedData] = RT_spm_reslice2(memAlignObject, vol.mat, vol.dim, volData);
	vol.mat = memAlignObject.mat;   % transfer the matrix where the image was resliced to
    
	%{
	% write
	VO         = vol;
	VO.fname   = spm_file(vol.fname, 'prefix', 'r');
	VO.dim     = vol.dim(1:3);
	VO.dt      = vol.dt;
	VO.pinfo   = vol.pinfo;
	VO.mat     = memAlignObject.mat;
	VO.descrip = 'realigned resliced';
	VO = spm_write_vol(VO, reslicedData);
	%}
	
end
