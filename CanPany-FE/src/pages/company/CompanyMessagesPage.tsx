import { useEffect, useRef, useState } from 'react';
import type { KeyboardEvent } from 'react';
import { useParams } from 'react-router-dom';
import { isAxiosError } from 'axios';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { MessageSquare, Send } from 'lucide-react';
import toast from 'react-hot-toast';
import { Button, Card } from '../../components/ui';
import { messagesApi } from '../../api/messages.api';
import type { Message } from '../../api/messages.api';
import {
    CompanyWorkspaceErrorState,
    CompanyWorkspaceLoader,
    EmptyState,
    SectionHeader,
} from '../../components/features/companies';
import { useCompanyWorkspace } from '../../hooks/company/useCompanyWorkspace';
import { messageKeys } from '../../lib/queryKeys';
import { useAuthStore } from '../../stores/auth.store';
import { formatRelativeTime } from '../../utils';
import { useTranslation } from 'react-i18next';

// BE does not expose a WebSocket/SignalR hub; we fall back to REST polling.
// 3 s is a reasonable tradeoff between perceived responsiveness and server load.
const POLLING_INTERVAL_MS = 3_000;

export function CompanyMessagesPage() {
    const { conversationId } = useParams<{ conversationId: string }>();
    const queryClient = useQueryClient();
    const { user } = useAuthStore();
    const { t } = useTranslation('company');
    const [draft, setDraft] = useState('');

    // Invisible anchor element at the bottom of the message list used for auto-scroll.
    const bottomAnchorRef = useRef<HTMLDivElement>(null);

    const {
        isLoading: isWorkspaceLoading,
        hasFatalError,
    } = useCompanyWorkspace();

    // ── Messages (polling) ────────────────────────────────────────────────────
    const messagesQuery = useQuery({
        queryKey: messageKeys.byConversation(conversationId ?? ''),
        queryFn: () => messagesApi.getMessages(conversationId!),
        enabled: !!conversationId,
        refetchInterval: POLLING_INTERVAL_MS,
        // Keep the previous snapshot visible while a new fetch is in flight so
        // the list doesn't flash empty.
        placeholderData: (previousData) => previousData,
    });

    // ── Mark conversation as read ─────────────────────────────────────────────
    // Fire-and-forget — a read-receipt failure should never block the user.
    const markReadMutation = useMutation({
        mutationFn: () => messagesApi.markConversationAsRead(conversationId!),
    });

    useEffect(() => {
        if (!conversationId) return;
        markReadMutation.mutate();
        // We intentionally omit markReadMutation from deps to avoid re-firing
        // every time the mutation instance changes reference.
    }, [conversationId]); // eslint-disable-line react-hooks/exhaustive-deps

    // ── Auto-scroll to bottom when the messages list grows ───────────────────
    useEffect(() => {
        bottomAnchorRef.current?.scrollIntoView({ behavior: 'smooth' });
    }, [messagesQuery.data]);

    // ── Send message ──────────────────────────────────────────────────────────
    const sendMutation = useMutation({
        mutationFn: (text: string) => messagesApi.sendMessage(conversationId!, text),

        onMutate: async (text) => {
            // Cancel any in-flight refetch so it does not overwrite our optimistic update.
            await queryClient.cancelQueries({
                queryKey: messageKeys.byConversation(conversationId!),
            });

            const previousMessages = queryClient.getQueryData<Message[]>(
                messageKeys.byConversation(conversationId!)
            );

            // Append a temporary optimistic message immediately so the user
            // sees their text without waiting for the server round-trip.
            queryClient.setQueryData<Message[]>(
                messageKeys.byConversation(conversationId!),
                (current = []) => [
                    ...current,
                    {
                        id: `optimistic-${Date.now()}`,
                        conversationId: conversationId!,
                        senderId: user?.id ?? '',
                        text,
                        isRead: false,
                        createdAt: new Date().toISOString(),
                    },
                ]
            );

            // Clear the input straight away so it feels snappy.
            setDraft('');

            return { previousMessages };
        },

        onError: (_error, _text, context) => {
            // Roll back the optimistic message if the server rejected the request.
            queryClient.setQueryData(
                messageKeys.byConversation(conversationId!),
                context?.previousMessages
            );

            const message = isAxiosError(_error)
                ? _error.response?.data?.message || t('messages.toastSendFailed')
                : t('messages.toastSendFailed');
            toast.error(message);
        },

        onSuccess: () => {
            // Replace the optimistic entry with the real persisted message.
            queryClient.invalidateQueries({
                queryKey: messageKeys.byConversation(conversationId!),
                exact: true,
            });
        },
    });

    const handleSend = () => {
        const text = draft.trim();
        if (!text || sendMutation.isPending) return;
        sendMutation.mutate(text);
    };

    // Enter sends, Shift+Enter inserts a newline — standard chat UX convention.
    const handleKeyDown = (event: KeyboardEvent<HTMLTextAreaElement>) => {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            handleSend();
        }
    };

    // ── Guards ────────────────────────────────────────────────────────────────
    if (isWorkspaceLoading) return <CompanyWorkspaceLoader />;

    if (hasFatalError) {
        return (
            <CompanyWorkspaceErrorState
                title={t('messages.errorLoadTitle')}
                description={t('messages.errorLoadDesc')}
                icon={<MessageSquare className="h-6 w-6" />}
            />
        );
    }

    // No conversationId in the URL: the user landed here without context.
    // They should arrive from an application detail page which provides the id.
    if (!conversationId) {
        return (
            <div className="space-y-6">
                <SectionHeader title={t('messages.title')} description={t('messages.descriptionLanding')} />
                <EmptyState
                    title={t('messages.emptyNoConversationTitle')}
                    description={t('messages.emptyNoConversationDesc')}
                    icon={<MessageSquare className="h-6 w-6" />}
                />
            </div>
        );
    }

    const messages = messagesQuery.data ?? [];
    const isFetchError = !!messagesQuery.error && !messagesQuery.data;

    return (
        <div className="flex h-[calc(100vh-10rem)] flex-col gap-4">
            <SectionHeader
                title={t('messages.title')}
                description={t('messages.descriptionList')}
                backLink="/company/applications"
                backLabel={t('messages.backToApplications')}
            />

            <Card className="flex flex-1 flex-col overflow-hidden p-0">
                {/* ── Message list ─────────────────────────────────────────── */}
                <div className="flex-1 overflow-y-auto p-6">
                    {messagesQuery.isLoading && !messagesQuery.data ? (
                        <div className="flex h-full items-center justify-center">
                            <div className="h-8 w-8 animate-spin rounded-full border-b-2 border-[#00b14f]" />
                        </div>
                    ) : isFetchError ? (
                        <div className="flex h-full items-center justify-center">
                            <p className="text-sm text-red-500">{t('messages.loadError')}</p>
                        </div>
                    ) : messages.length === 0 ? (
                        <div className="flex h-full items-center justify-center">
                            <p className="text-sm text-gray-400">{t('messages.emptyThread')}</p>
                        </div>
                    ) : (
                        <div className="space-y-3">
                            {messages.map((message) => (
                                <MessageBubble
                                    key={message.id}
                                    message={message}
                                    isSelf={message.senderId === user?.id}
                                    sendingLabel={t('messages.sendingOptimistic')}
                                />
                            ))}
                        </div>
                    )}

                    {/* Scrolled into view whenever the message list updates */}
                    <div ref={bottomAnchorRef} />
                </div>

                {/* ── Input area ───────────────────────────────────────────── */}
                <div className="border-t border-gray-100 p-4">
                    <div className="flex items-end gap-3">
                        <textarea
                            rows={2}
                            value={draft}
                            onChange={(e) => setDraft(e.target.value)}
                            onKeyDown={handleKeyDown}
                            placeholder={t('messages.inputPlaceholder')}
                            disabled={sendMutation.isPending}
                            className="flex-1 resize-none rounded-lg border border-gray-300 px-4 py-3 text-sm text-gray-900 outline-none transition focus:border-[#00b14f] focus:ring-2 focus:ring-[#00b14f]/20 disabled:bg-gray-50"
                        />
                        <Button
                            onClick={handleSend}
                            disabled={!draft.trim() || sendMutation.isPending}
                            isLoading={sendMutation.isPending}
                            aria-label={t('messages.sendAriaLabel')}
                        >
                            <Send className="h-4 w-4" />
                            {t('messages.sendButton')}
                        </Button>
                    </div>
                    <p className="mt-2 text-xs text-gray-400">{t('messages.composeHint')}</p>
                </div>
            </Card>
        </div>
    );
}

// ── MessageBubble ─────────────────────────────────────────────────────────────
// Extracted as a standalone component to avoid re-rendering the entire list
// when only the draft input changes.

interface MessageBubbleProps {
    message: Message;
    /** True when the message was sent by the currently logged-in user. */
    isSelf: boolean;
    sendingLabel: string;
}

function MessageBubble({ message, isSelf, sendingLabel }: MessageBubbleProps) {
    const isOptimistic = message.id.startsWith('optimistic-');

    return (
        <div className={`flex ${isSelf ? 'justify-end' : 'justify-start'}`}>
            <div
                className={[
                    'max-w-[70%] rounded-2xl px-4 py-2.5 transition-opacity',
                    isSelf
                        ? 'bg-[#00b14f] text-white'
                        : 'bg-gray-100 text-gray-900',
                    // Optimistic messages are slightly faded until the server confirms them.
                    isOptimistic ? 'opacity-60' : 'opacity-100',
                ].join(' ')}
            >
                <p className="whitespace-pre-wrap text-sm leading-6">{message.text}</p>
                <p
                    className={`mt-1 text-right text-[10px] ${
                        isSelf ? 'text-white/70' : 'text-gray-400'
                    }`}
                >
                    {isOptimistic ? sendingLabel : formatRelativeTime(message.createdAt)}
                </p>
            </div>
        </div>
    );
}
