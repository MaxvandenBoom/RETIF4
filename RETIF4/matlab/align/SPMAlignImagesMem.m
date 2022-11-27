function [reslicedData, rparams] = SPMAlignImageMem(memAlignObject, files)
	
	% test data
	%{
	referenceFilePath = 'D:\\G4\\realignment_volume.img';
	refP = spm_vol(referenceFilePath);
	memAlignObject = RT_spm_realign_mem_createAlignObject(refP);
	files = {};
	for i = 1:20
		files{i} = ['D:\G5\cr_Timmy(', num2str(i), ').img'];
	end
	%}
	
	% load volumes (does not load data, just info)
	P = [];
	for i = 1:length(files)
		P = horzcat(P, spm_vol(files{i}));
	end
	
	% realign
	[P, rparams] = RT_spm_realign_mem(memAlignObject, P);
	
	% reslice
	P = [refP, P];
	[reslicedData, reslicedMat] = RT_spm_reslice(P);
	
	%{
	% write
	for i = 2:length(P)
		VO         = P(i);
		VO.fname   = spm_file(P(i).fname, 'prefix', 'realigned_');
		VO.dim     = P(1).dim(1:3);
		VO.dt      = P(i).dt;
		VO.pinfo   = P(i).pinfo;
		VO.mat     = reslicedMat;
		VO.descrip = 'realigned resliced';
		VO = spm_write_vol(VO, reslicedData(:, :, :, i - 1));
	end
	%}
	
end
