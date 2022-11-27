function [reslicedData, rparams] = SPMAlignImages(referenceFilePath, files)
	
	%{
	% test data
	referenceFilePath = 'D:\\G4\\realignment_volume.img';
	files = {};
	for i = 1:20
		files{i} = ['D:\G5\cr_Timmy(', num2str(i), ').img'];
	end
	%}
	
	% load (does not load data, just info)
	P = spm_vol(referenceFilePath);
	for i = 1:length(files)
		P = horzcat(P, spm_vol(files{i}));
	end
	
	% realign
	[P, rparams] = RT_spm_realign(P);
	
	% reslice
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
	
	%{
	% old using SPM image load/write
	% prepare the use of spm
	spm('defaults', 'fmri');
	spm_jobman('initcfg');
	
	% set (est+write) align parameters
	clear matlabbatch;
	matlabbatch{1}.spm.spatial.realign.estwrite.data = { files }';
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.quality = 0.9;
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.sep = 4;
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.fwhm = 5;
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.rtm = 0;
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.interp = 2;	% 2nd degree B-spline
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.wrap = [0 0 0];
	matlabbatch{1}.spm.spatial.realign.estwrite.eoptions.weight = '';
	matlabbatch{1}.spm.spatial.realign.estwrite.roptions.which = [2 0];
	matlabbatch{1}.spm.spatial.realign.estwrite.roptions.interp = 4;	% 4th degree B-spline
	matlabbatch{1}.spm.spatial.realign.estwrite.roptions.wrap = [0 0 0];
	matlabbatch{1}.spm.spatial.realign.estwrite.roptions.mask = 1;
	matlabbatch{1}.spm.spatial.realign.estwrite.roptions.prefix = 'realigned_';

	% run the (est+write) align
	spm_jobman('run', matlabbatch);
	%}
	
end