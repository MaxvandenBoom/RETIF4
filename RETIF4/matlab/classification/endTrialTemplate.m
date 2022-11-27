predictedLabel = 0;

if (trStart > 0 && size(rtData, 2) > 0)

	% store the sample index at end of the trial
	trEnd = size(rtData, 2);

	% update the stop index in the list of start and stop indices
	rtDataTrialIndices(size(rtDataTrialIndices,1), 2) = trEnd;

	% extract the data to classify on
	trData = rtData(:,trStart:end);

	% store the data to classify on in the list of trials
	rtDataTrials{length(rtDataTrials) + 1,1} = trData;

	% determine the means (over time)
	trDataMean = mean(trData, 1);
	
	% determine the highest value
	[~, I] = max(trDataMean);
	
	% retrieve the peakData and transpose
	peakData = trData(:, I)';
	
	% store the peak mean
	peakMean = mean(peakData);
	rtDataTrialMaxMeans = vertcat(rtDataTrialMaxMeans, peakMean);
	
	% correlate the trial with all templates
	crosscorr_sym0 = corr(testTrial, sym0Template);
	crosscorr_sym1 = corr(testTrial, sym1Template);
	crosscorr_sym2 = corr(testTrial, sym2Template);	
	if (crosscorr_sym0 < 0)
		crosscorr_sym0 = crosscorr_sym0 * -1;
	end
	if (crosscorr_sym1 < 0)
		crosscorr_sym1 = crosscorr_sym1 * -1;
	end    
	if (crosscorr_sym2 < 0)
		crosscorr_sym2 = crosscorr_sym2 * -1;
	end
	
	% classify on the highest correlating template
	[~,predictedLabel] = max([crosscorr_sym0, crosscorr_sym1, crosscorr_sym2]);

end