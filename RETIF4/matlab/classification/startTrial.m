
% store the sample index at start of the trial
trStart = size(rtData, 2);

% store it also in the list of start and stop indices
rtDataTrialIndices = vertcat(rtDataTrialIndices, [trStart, trStart]);