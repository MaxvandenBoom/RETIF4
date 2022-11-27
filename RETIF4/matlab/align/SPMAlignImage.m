function [reslicedData, rparams] = SPMAlignImage(referenceFilePath, file)
	
	%{
	% test data
	referenceFilePath = 'D:\\G4\\realignment_volume.img';
	file = 'D:\\G5\\cr_Timmy(1).img';
	%}
	
	% load (does not load data, just info)
    P = [spm_vol(referenceFilePath); spm_vol(file)];

	% realign
	[P, rparams] = RT_spm_realign(P);
	
	% reslice
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
	
	%{	
	% old using SPM image load/write
	% set (est+write) align parameters
	clear alignbatch;
	alignbatch{1}.spm.spatial.realign.estwrite.data = { {referenceFilePath; file} };
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.quality = 0.9;
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.sep = 4;
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.fwhm = 5;
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.rtm = 0;
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.interp = 1;		% 1 is lowest while not breaking
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.wrap = [0 0 0];
	alignbatch{1}.spm.spatial.realign.estwrite.eoptions.weight = '';
	alignbatch{1}.spm.spatial.realign.estwrite.roptions.which = [1 0];
	alignbatch{1}.spm.spatial.realign.estwrite.roptions.interp = 1;		% 1 is lowest while not breaking
	alignbatch{1}.spm.spatial.realign.estwrite.roptions.wrap = [0 0 0];
	alignbatch{1}.spm.spatial.realign.estwrite.roptions.mask = 1;
	alignbatch{1}.spm.spatial.realign.estwrite.roptions.prefix = 'r';

	% run the (est+write) align
	spm_jobman('run', alignbatch);
	%}
	
end
