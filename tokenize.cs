using System; 

namespace CC9
{
    public partial class Program
    {
        // トークンの種類
        enum TokenKind{
            TK_RESERVED, // 記号
            /// <summary>識別子/summary>
            TK_IDENT,    // 識別子
            TK_NUM,      // 整数トークン
            TK_EOF,      // 入力の終わりを表すトークン
        };

        class Token {
            public TokenKind kind; // トークンの型
            public  int next;    // 次の入力トークン
            public int val;        // kindがTK_NUMの場合、その数値
            public string? str;      // トークン文字列
            public int len;        // トークンの長さ
           
        };
        
        // 入力文字列pをトークナイズしてそれを返す
        // 入力文字列をトークナイズしてトークンリストを返す
        /// <summary>トークナイズする</summary>
        /// <param name="p">入力文字列</param>
        /// <returns>トークンリスト</returns>
        static List<Token> tokenize(string p) {
            int next = 1;
            Token cur = new();
            List<Token> tokenList =new List<Token>();

            var cs = p.ToCharArray();
            int i= 0;
            while(i<cs.Length) {
                // 空白文字をスキップ
                if (isspace(cs[i].ToString())) {
                    i++;
                    continue;
                }

                // Single-letter identfer
                if (isidentfer(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_IDENT, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                // Multi-letter punctuator
                if (iscomparisonhead(cs[i].ToString())) {
                    if(i+1<cs.Length){
                        var comp =cs[i].ToString()+cs[i+1].ToString();
                        if (iscomparison(comp)) {
                            cur = new_token(TokenKind.TK_RESERVED, next, comp,2);
                            tokenList.Add(cur);
                            next++;
                            i+=2;
                            continue;
                        }
                    }
                }

                // Single-letter punctuator
                if (isarithmetic(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                if (isdigit(cs[i].ToString())) {
        
                    string numberStr = cs[i].ToString();
                    i++;
                    if(i<cs.Length){
                        while(isdigit(cs[i].ToString())) {
                            numberStr+= cs[i].ToString();
                            i++;
                            if(i>=cs.Length)break;
                        }
                    }
                    cur = new_token(TokenKind.TK_NUM,next, numberStr,i);
                    cur.val = strtol(numberStr);
                    tokenList.Add(cur);
                    next++;
                    continue;
                }

                // assign
                if (isassign(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                // statement terminater
                if (isterminater(cs[i].ToString())) {
                    cur = new_token(TokenKind.TK_RESERVED, next, cs[i].ToString(),1);
                    tokenList.Add(cur);
                    next++;
                    i++;
                    continue;
                }

                //ここに進んだら例外
                Console.Write($"トークナイズできません\n");
                i++;
            }

            cur = new_token(TokenKind.TK_EOF, next, "",0);
            tokenList.Add(cur);
            return tokenList;
        }

    }
}